using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using GameFrameX.Core.Service.Tenant;

namespace GameFrameX.Advertisement.Abstract;

/// <summary>
/// 广告回调配置 ConfigJson 敏感字段级加解密收口（C371 / GFX-838，对齐 C370 TenantObjectStorageConfigCrypto）：
/// 写入侧在 NormalizeConfigJson 规范化之后、落库之前调用 <see cref="EncryptConfigJson"/>；
/// 读取侧在构建回调验签上下文之前调用 <see cref="DecryptConfigJson"/>。
/// <para>敏感字段集合由 Provider Schema 的 <c>Sensitive</c> 标记或 <c>Type == "secret"</c> 驱动（与表单脱敏 / 必填校验同源判定），
/// 非敏感字段（callbackUrl 等）保持明文可读；字段值为密文形态（<c>enc:sec:v1:</c> 前缀）时幂等跳过。</para>
/// <para>性能约束：底层 PBKDF2 派生单次数百毫秒级——<see cref="DecryptConfigJson"/> 按「密文原文 → 明文 JSON」做进程级缓存，
/// 同一密文只解密一次；任何配置写入都会产生新密文（每次加密随机 Salt + Nonce），缓存 key 随之失效。</para>
/// <para>ponytail: 缓存字典无淘汰上限（key 为整段密文 JSON）；广告回调配置行数极少（每租户个位数），增长天花板可忽略。
/// 升级路径：按配置行 Id 失效或换 LRU。</para>
/// </summary>
public static class AdvertisementCallbackConfigCrypto
{
    private static readonly AdvertisementCallbackConfigSchemaProvider SchemaProvider = new();
    private static readonly ConcurrentDictionary<string, AdvertisementCallbackConfigSchema?> SchemaCache = new(StringComparer.Ordinal);

    /// <summary>
    /// 密文 ConfigJson → 明文 ConfigJson 的进程级解密缓存（回调验签每请求读取，禁止每请求 PBKDF2 派生）。
    /// </summary>
    private static readonly ConcurrentDictionary<string, string> DecryptCache = new(StringComparer.Ordinal);

    /// <summary>
    /// 对规范化后的 ConfigJson 做 schema 驱动敏感字段加密。空 / 畸形 JSON 原样返回（容错语义与 NormalizeConfigJson 一致）；
    /// 已加密字段幂等跳过（存量密文字段重写不换密文）。
    /// </summary>
    /// <param name="provider">广告提供商名称（决定敏感字段集合）</param>
    /// <param name="configJson">规范化后的明文 ConfigJson</param>
    /// <returns>敏感字段已加密的 ConfigJson（或无可加密内容时的原值）</returns>
    public static string EncryptConfigJson(string provider, string configJson)
    {
        var sensitiveKeys = GetSensitiveKeys(provider);
        if (sensitiveKeys.Count == 0)
        {
            return configJson;
        }

        return TransformSensitiveFields(configJson, sensitiveKeys, TenantSecretCrypto.Encrypt);
    }

    /// <summary>
    /// 对落库的 ConfigJson 做 schema 驱动敏感字段解密（供回调验签 / 投放上下文构建）。
    /// 无密文字段的 JSON 原样返回；含密文字段的 JSON 解密并经进程级缓存，同一密文只派生一次。
    /// </summary>
    /// <param name="provider">广告提供商名称</param>
    /// <param name="configJson">落库形态的 ConfigJson（敏感字段可能为密文）</param>
    /// <returns>敏感字段为明文的 ConfigJson</returns>
    public static string DecryptConfigJson(string provider, string configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson) || !configJson.Contains(TenantSecretCrypto.EncryptedPrefix, StringComparison.Ordinal))
        {
            return configJson;
        }

        // 缓存 key 必须含 provider：敏感字段集合由 schema 决定，同一密文字符串在不同 provider 下的解密结果不同
        // ponytail: 并发首次 miss 会各自解密一次后竞争写入（最后者胜），浪费一次派生但不产生错误数据；回调读取并发度低，不值得加锁。
        return DecryptCache.GetOrAdd(provider + "\n" + configJson, _ => DecryptCore(provider, configJson));
    }

    private static string DecryptCore(string provider, string configJson)
    {
        var sensitiveKeys = GetSensitiveKeys(provider);
        if (sensitiveKeys.Count == 0)
        {
            return configJson;
        }

        return TransformSensitiveFields(configJson, sensitiveKeys, TenantSecretCrypto.Decrypt);
    }

    /// <summary>
    /// 按敏感字段 key 集合对 JSON 字符串逐字段应用变换（加密或解密）；无字符串型敏感字段命中时返回原字符串。
    /// </summary>
    private static string TransformSensitiveFields(string configJson, List<string> sensitiveKeys, Func<string?, string?> transform)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            return configJson;
        }

        JsonObject? node;
        try
        {
            node = JsonNode.Parse(configJson)?.AsObject();
        }
        catch (Exception)
        {
            return configJson;
        }

        if (node == null)
        {
            return configJson;
        }

        var changed = false;
        foreach (var key in sensitiveKeys)
        {
            if (!node.TryGetPropertyValue(key, out var value) || value is not JsonValue text || !text.TryGetValue<string>(out var raw) || string.IsNullOrEmpty(raw))
            {
                continue;
            }

            var transformed = transform(raw);
            if (!string.Equals(transformed, raw, StringComparison.Ordinal))
            {
                node[key] = transformed;
                changed = true;
            }
        }

        return changed ? node.ToJsonString() : configJson;
    }

    /// <summary>
    /// 取 Provider Schema 的敏感字段 key 集合（进程级缓存，反射只做一次）。判定与表单脱敏同源：
    /// <c>Sensitive</c> 标记或 <c>Type == "secret"</c>。未注册 Provider / 无 Schema 返回空集合（直通）。
    /// </summary>
    private static List<string> GetSensitiveKeys(string provider)
    {
        var schema = SchemaCache.GetOrAdd(provider, key => SchemaProvider.GetSchema(key));
        if (schema == null)
        {
            return new List<string>();
        }

        return schema.Fields.Where(field => field.Sensitive || field.Type == "secret").Select(field => field.Key).ToList();
    }
}
