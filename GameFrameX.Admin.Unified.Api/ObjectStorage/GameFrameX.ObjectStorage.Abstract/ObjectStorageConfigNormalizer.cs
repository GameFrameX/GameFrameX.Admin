using System.Text.Json;
using System.Text.Json.Nodes;

namespace GameFrameX.ObjectStorage.Abstract;

/// <summary>
/// ConfigJson 规范化与脱敏工具，服务于 CRUD 服务的 Add/Update/Page/Detail。
/// 规则：
/// 1. 白名单：仅保留 Schema 内声明字段，未知键一律丢弃；
/// 2. 必填：Required 字段空值拒绝；
/// 3. 敏感保留：敏感字段提交 <see cref="ObjectStorageConfigMask.Placeholder"/> 时，回退到旧配置中的原值；
/// 4. 脱敏：Page 整体覆盖为掩码；Detail 仅敏感字段替换为掩码，其余保留。
/// </summary>
public static class ObjectStorageConfigNormalizer
{
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = false,
    };

    /// <summary>
    /// 规范化 ConfigJson。返回白名单内、已应用 ****** 保留语义的干净 JSON 字符串。
    /// </summary>
    public static string Normalize(
        IObjectStorageConfigSchemaProvider schemaProvider,
        string provider,
        string configJson,
        string oldConfigJson)
    {
        if (schemaProvider == null)
        {
            throw new ArgumentNullException(nameof(schemaProvider));
        }

        var schema = schemaProvider.GetSchema(provider);
        if (schema == null)
        {
            throw new ArgumentException("未注册的对象存储 Provider：" + provider, nameof(provider));
        }

        var input = ParseObject(configJson);
        var old = ParseObject(oldConfigJson);
        var result = new JsonObject();

        foreach (var field in schema.Fields)
        {
            JsonNode? value = null;
            if (input != null && input.TryGetPropertyValue(field.Key, out var inputNode) && !IsEmptyNode(inputNode))
            {
                value = inputNode;
            }
            else if (old != null && old.TryGetPropertyValue(field.Key, out var oldNode) && !IsEmptyNode(oldNode))
            {
                value = oldNode;
            }

            if (field.Sensitive && IsMaskValue(value))
            {
                if (old != null && old.TryGetPropertyValue(field.Key, out var oldNode2) && !IsEmptyNode(oldNode2))
                {
                    value = oldNode2;
                }
                else
                {
                    value = null;
                }
            }

            if (field.Required && IsEmptyNode(value))
            {
                throw new ArgumentException($"字段 {field.Label}（{field.Key}）必填");
            }

            if (value != null)
            {
                result[field.Key] = value.DeepClone();
            }
        }

        return result.ToJsonString(WriteOptions);
    }

    /// <summary>整体脱敏：直接返回掩码占位符，覆盖整段 ConfigJson。</summary>
    public static string Mask()
    {
        return ObjectStorageConfigMask.Placeholder;
    }

    /// <summary>
    /// 字段级脱敏：敏感字段替换为掩码，其余字段保留原值；返回新 JSON 字符串。
    /// </summary>
    public static string MaskFields(IObjectStorageConfigSchemaProvider schemaProvider, string provider, string configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            return configJson;
        }

        var node = ParseObject(configJson);
        if (node == null)
        {
            return configJson;
        }

        var schema = schemaProvider != null ? schemaProvider.GetSchema(provider) : null;
        if (schema != null)
        {
            foreach (var field in schema.Fields)
            {
                if (!field.Sensitive)
                {
                    continue;
                }

                if (node.TryGetPropertyValue(field.Key, out _) && !IsEmptyNode(node[field.Key]))
                {
                    node[field.Key] = ObjectStorageConfigMask.Placeholder;
                }
            }
        }

        return node.ToJsonString(WriteOptions);
    }

    private static JsonObject? ParseObject(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            // JsonNode.Parse 对畸形 JSON 抛 JsonException；AsObject() 对合法但非对象（数组/标量）抛 InvalidOperationException。
            // 两者都视为“无可效仿的配置对象”，返回 null 让上层 required 校验接管，避免 500。
            return JsonNode.Parse(json)?.AsObject();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static bool IsEmptyNode(JsonNode? node)
    {
        if (node == null)
        {
            return true;
        }

        return node is JsonValue value && value.TryGetValue<string>(out var text) && string.IsNullOrEmpty(text);
    }

    private static bool IsMaskValue(JsonNode? node)
    {
        return node != null
               && node is JsonValue value
               && value.TryGetValue<string>(out var text)
               && text == ObjectStorageConfigMask.Placeholder;
    }
}