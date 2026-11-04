using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace GameFrameX.Core.Logging;

/// <summary>日志专用凭据脱敏；从快照创建副本，不修改业务请求或响应。</summary>
public static class SensitiveLogRedactor
{
    public const string Mask = GameFrameX.Core.Service.SensitiveFieldMask.MaskValue;

    public static bool IsSensitiveName(string name)
    {
        var normalized = NormalizeFieldName(name);
        return normalized.Contains("password") || normalized.Contains("secret") || normalized.Contains("token")
            || normalized.Contains("privatekey") || normalized.Contains("signingkey") || normalized.Contains("sessionkey")
            || normalized.Contains("encryptionkey") || normalized.Contains("credential") || normalized.Contains("connectionstring")
            || normalized.Contains("apikey") || normalized.Contains("signkey") || normalized.Contains("verificationkey") || normalized.Contains("keymaterial")
            || normalized is "key" or "keyvalue" or "authorization" or "accesskey";
    }

    private static string NormalizeFieldName(string name)
    {
        return new string(name.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
    }

    public static string? Redact(string? text)
    {
        if (string.IsNullOrEmpty(text) || text == Mask)
        {
            return text;
        }

        return RedactText(text, 0, new HashSet<string>(StringComparer.Ordinal));
    }

    private static string RedactText(string text, int depth, HashSet<string> secrets)
    {
        if (depth >= 32)
        {
            return Mask;
        }

        try
        {
            var node = JsonNode.Parse(text);
            if (node == null)
            {
                return text;
            }

            if (node is JsonValue scalar && scalar.TryGetValue<string>(out var nested))
            {
                var redacted = RedactText(nested, depth + 1, secrets);
                return nested == redacted ? text : JsonSerializer.Serialize(redacted);
            }

            var changed = RedactNode(node, secrets, depth + 1);
            changed |= ReplaceSecretValues(node, secrets);
            return changed ? node.ToJsonString() : text;
        }
        catch (JsonException)
        {
            // 不把残缺 JSON、URL 查询或未结构化凭据文本原样持久化。
            return text.TrimStart().StartsWith('{') || text.TrimStart().StartsWith('[')
                || Regex.IsMatch(text, "(password|secret|token|private.?key|signing.?key|session.?key|authorization|credential)[a-z0-9_]*[^a-z0-9_]*[:=]", RegexOptions.IgnoreCase)
                ? Mask : text;
        }
    }

    private static bool ReplaceSecretValues(JsonNode node, HashSet<string> secrets)
    {
        var changed = false;
        if (node is JsonObject dictionary)
        {
            foreach (var pair in dictionary.ToArray())
            {
                if (pair.Value is JsonValue value && value.TryGetValue<string>(out var text))
                {
                    var redacted = ReplaceSecrets(text, secrets);
                    if (redacted != text)
                    {
                        dictionary[pair.Key] = redacted;
                        changed = true;
                    }
                }
                else if (pair.Value != null)
                {
                    changed |= ReplaceSecretValues(pair.Value, secrets);
                }
            }
        }
        else if (node is JsonArray array)
        {
            for (var index = 0; index < array.Count; index++)
            {
                if (array[index] is JsonValue value && value.TryGetValue<string>(out var text))
                {
                    var redacted = ReplaceSecrets(text, secrets);
                    if (redacted != text)
                    {
                        array[index] = redacted;
                        changed = true;
                    }
                }
                else if (array[index] != null)
                {
                    changed |= ReplaceSecretValues(array[index]!, secrets);
                }
            }
        }

        return changed;
    }

    private static string ReplaceSecrets(string text, HashSet<string> secrets)
    {
        foreach (var secret in secrets.Where(value => value.Length > 0 && value != Mask).OrderByDescending(value => value.Length))
        {
            text = Regex.Replace(text, Regex.Escape(Mask) + "|" + Regex.Escape(secret), _ => Mask);
        }

        return text;
    }

    private static bool RedactNode(JsonNode node, HashSet<string> secrets, int depth)
    {
        var changed = false;
        if (node is JsonObject dictionary)
        {
            // Monitor 参数/claims 和 SqlSugar 差异列以 name/type/ColumnName + value 成对存储。
            var sensitiveValue = dictionary.Any(pair => NormalizeFieldName(pair.Key) is "name" or "type" or "columnname" or "dbcolumnname" or "parametername" or "code"
                && pair.Value is JsonValue value && value.TryGetValue<string>(out var descriptor) && IsSensitiveName(descriptor));
            foreach (var pair in dictionary.ToArray())
            {
                if (IsSensitiveName(pair.Key) || (sensitiveValue && NormalizeFieldName(pair.Key) is "value" or "oldvalue" or "newvalue"))
                {
                    if (pair.Value is JsonValue value && value.TryGetValue<string>(out var secret))
                    {
                        secrets.Add(secret);
                    }

                    if (pair.Value?.ToJsonString() != JsonValue.Create(Mask)!.ToJsonString())
                    {
                        dictionary[pair.Key] = Mask;
                        changed = true;
                    }
                }
                else if (pair.Value is JsonValue scalar && scalar.TryGetValue<string>(out var text))
                {
                    var redacted = RedactText(text, depth, secrets);
                    if (redacted != text)
                    {
                        dictionary[pair.Key] = redacted;
                        changed = true;
                    }
                }
                else if (pair.Value != null)
                {
                    changed |= RedactNode(pair.Value, secrets, depth + 1);
                }
            }
        }
        else if (node is JsonArray array)
        {
            for (var index = 0; index < array.Count; index++)
            {
                if (array[index] is JsonValue value && value.TryGetValue<string>(out var text))
                {
                    var redacted = RedactText(text, depth, secrets);
                    if (redacted != text)
                    {
                        array[index] = redacted;
                        changed = true;
                    }
                }
                else if (array[index] != null)
                {
                    changed |= RedactNode(array[index]!, secrets, depth + 1);
                }
            }
        }

        return changed;
    }
}
