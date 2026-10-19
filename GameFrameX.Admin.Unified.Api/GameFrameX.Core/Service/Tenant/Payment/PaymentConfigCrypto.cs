using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json.Nodes;
using GameFrameX.Payment.Abstract;

namespace GameFrameX.Core.Service.Tenant;

/// <summary>
/// 支付配置 ConfigJson 敏感字段级加解密收口（C371 / GFX-838，对齐 C370 TenantObjectStorageConfigCrypto）：
/// 写入侧在 ValidateConfigJson 校验之后、落库之前调用 <see cref="EncryptConfigJson"/>；
/// 读取侧（PaymentOptionSettingProvider 反序列化前）调用 <see cref="DecryptConfigJson"/>。
/// <para>敏感字段集合由渠道 Option 类的 <c>[PaymentField(Sensitive = true)]</c> 标注驱动（与表单 schema 反射同源），
/// 覆盖 Sensitive+Multiline 的私钥 / 证书字段（PaymentConfigField.Type 会把这类字段显示为 multiline，
/// 不能按 Type == "secret" 判定，必须回到 Attribute 事实源）；非敏感字段保持明文可读；
/// 字段值为密文形态（<c>enc:sec:v1:</c> 前缀）时幂等跳过。</para>
/// <para>未注册渠道（如 Wechat 历史配置行）无 Option 类型可反射，返回空集合直通——维持存量明文形态，
/// 解密侧对非密文同样直通，读写两侧语义闭合。</para>
/// <para>性能约束：底层 PBKDF2 派生单次数百毫秒级——<see cref="DecryptConfigJson"/> 按「密文原文 → 明文 JSON」做进程级缓存，
/// 同一密文只解密一次；任何配置写入都会产生新密文（每次加密随机 Salt + Nonce），缓存 key 随之失效。</para>
/// <para>ponytail: 缓存字典无淘汰上限（key 为整段密文 JSON）；支付配置行数极少（每应用每渠道个位数），增长天花板可忽略。
/// 升级路径：按配置行 Id 失效或换 LRU。</para>
/// </summary>
public static class PaymentConfigCrypto
{
    /// <summary>
    /// 密文 ConfigJson → 明文 ConfigJson 的进程级解密缓存（下单链路每请求读取，禁止每请求 PBKDF2 派生）。
    /// </summary>
    private static readonly ConcurrentDictionary<string, string> DecryptCache = new(StringComparer.Ordinal);

    /// <summary>
    /// 渠道 → 敏感字段 key 集合缓存（反射只做一次；空集合 = 无敏感字段或未注册渠道）。
    /// </summary>
    private static readonly ConcurrentDictionary<string, List<string>> SensitiveKeysCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 对校验通过的 ConfigJson 做渠道 schema 驱动敏感字段加密。空 / 畸形 JSON 原样返回（容错语义与 ValidateConfigJson 一致）；
    /// 已加密字段幂等跳过（存量密文字段重写不换密文）。
    /// </summary>
    /// <param name="payChannel">支付渠道名（小写规范形态，决定敏感字段集合）</param>
    /// <param name="configJson">校验通过的明文 ConfigJson</param>
    /// <returns>敏感字段已加密的 ConfigJson（或无可加密内容时的原值）</returns>
    public static string EncryptConfigJson(string payChannel, string configJson)
    {
        return TransformSensitiveFields(payChannel, configJson, TenantSecretCrypto.Encrypt);
    }

    /// <summary>
    /// 对落库的 ConfigJson 做渠道 schema 驱动敏感字段解密（供支付 Option 反序列化）。
    /// 无密文字段的 JSON 原样返回；含密文字段的 JSON 解密并经进程级缓存，同一密文只派生一次。
    /// </summary>
    /// <param name="payChannel">支付渠道名</param>
    /// <param name="configJson">落库形态的 ConfigJson（敏感字段可能为密文）</param>
    /// <returns>敏感字段为明文的 ConfigJson</returns>
    public static string DecryptConfigJson(string payChannel, string configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson) || !configJson.Contains(TenantSecretCrypto.EncryptedPrefix, StringComparison.Ordinal))
        {
            return configJson;
        }

        // 缓存 key 必须含渠道：敏感字段集合由渠道 Option 反射决定，同一密文字符串在不同渠道下的解密结果不同
        // ponytail: 并发首次 miss 会各自解密一次后竞争写入（最后者胜），浪费一次派生但不产生错误数据；下单读取并发度可控，不值得加锁。
        return DecryptCache.GetOrAdd(payChannel + "\n" + configJson, _ => TransformSensitiveFields(payChannel, configJson, TenantSecretCrypto.Decrypt));
    }

    /// <summary>
    /// 按渠道敏感字段 key 集合对 JSON 字符串逐字段应用变换（加密或解密）；无字符串型敏感字段命中时返回原字符串。
    /// </summary>
    private static string TransformSensitiveFields(string payChannel, string configJson, Func<string?, string?> transform)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            return configJson;
        }

        var sensitiveKeys = GetSensitiveKeys(payChannel);
        if (sensitiveKeys.Count == 0)
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
    /// 取渠道 Option 类标注 <c>[PaymentField(Sensitive = true)]</c> 的属性名集合（进程级缓存，反射只做一次）。
    /// 未注册渠道 / 无敏感字段返回空集合（直通）。
    /// </summary>
    private static List<string> GetSensitiveKeys(string payChannel)
    {
        return SensitiveKeysCache.GetOrAdd(payChannel ?? string.Empty, channel =>
        {
            var optionType = PaymentChannelRegistry.GetOptionType(channel);
            if (optionType == null)
            {
                return new List<string>();
            }

            return optionType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             .Where(property => property.GetCustomAttribute<PaymentFieldAttribute>() is { Sensitive: true })
                             .Select(property => property.Name)
                             .ToList();
        });
    }
}
