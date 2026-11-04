using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

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

        try
        {
            return RedactText(text, 0, new HashSet<string>(StringComparer.Ordinal));
        }
        catch
        {
            // 兜底 fail-closed：脱敏环节自身出错时丢弃内容保留掩码，绝不把异常或原文抛回调用方。
            return Mask;
        }
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
        catch (Exception exception) when (exception is JsonException or ArgumentException)
        {
            // 残缺 JSON、URL 查询或未结构化文本按凭据形态兜底扫描；以 { 或 [ 开头的结构残缺文本整体掩码。
            return RedactUnstructured(text);
        }
    }

    /// <summary>
    /// 非 JSON 文本的线性兜底扫描：定位 <c>:</c>/<c>=</c> 后向左取连续键名字符段，
    /// 归一化（含一次百分号解码）后与 <see cref="IsSensitiveName"/> 同源判定，命中即把值掩到引号闭合、下一个 &amp; 或行尾。
    /// 单次遍历、无回溯。
    /// </summary>
    private static string RedactUnstructured(string text)
    {
        var trimmed = text.TrimStart();
        if (trimmed.StartsWith('{') || trimmed.StartsWith('['))
        {
            // 结构残缺：无法确定值边界，整体 fail-closed。
            return Mask;
        }

        StringBuilder? result = null;
        var copiedLength = 0;
        for (var index = 0; index < text.Length; index++)
        {
            var current = text[index];
            if (current != ':' && current != '=')
            {
                continue;
            }

            var nameStart = index;
            while (nameStart > 0 && IsNameCharacter(text[nameStart - 1]))
            {
                nameStart--;
            }

            if (nameStart == index || !IsSensitiveUnstructuredName(text.Substring(nameStart, index - nameStart)))
            {
                continue;
            }

            var valueStart = index + 1;
            while (valueStart < text.Length && char.IsWhiteSpace(text[valueStart]) && text[valueStart] != '\n' && text[valueStart] != '\r')
            {
                valueStart++;
            }

            int valueEnd;
            if (valueStart < text.Length && text[valueStart] == '"')
            {
                valueEnd = valueStart + 1;
                while (valueEnd < text.Length && text[valueEnd] != '"')
                {
                    if (text[valueEnd] == '\\' && valueEnd + 1 < text.Length)
                    {
                        valueEnd++;
                    }

                    valueEnd++;
                }

                if (valueEnd < text.Length)
                {
                    valueEnd++;
                }
            }
            else
            {
                valueEnd = valueStart;
                while (valueEnd < text.Length && text[valueEnd] != '&' && text[valueEnd] != '\n' && text[valueEnd] != '\r')
                {
                    valueEnd++;
                }

                // 值已是掩码（上一轮结果）时跳过，避免把掩码后的同段内容再次吞并，保证幂等。
                if (valueEnd - valueStart >= Mask.Length && text.AsSpan(valueStart, Mask.Length).SequenceEqual(Mask))
                {
                    valueEnd = valueStart;
                }
            }

            if (valueEnd == valueStart)
            {
                continue;
            }

            result ??= new StringBuilder(text.Length);
            result.Append(text, copiedLength, valueStart - copiedLength);
            result.Append(Mask);
            copiedLength = valueEnd;
            index = valueEnd - 1;
        }

        if (result == null)
        {
            return text;
        }

        result.Append(text, copiedLength, text.Length - copiedLength);
        return result.ToString();
    }

    private static bool IsNameCharacter(char character)
    {
        return char.IsLetterOrDigit(character) || character == '_' || character == '%' || character == '-' || character == '.';
    }

    private static bool IsSensitiveUnstructuredName(string name)
    {
        if (IsSensitiveName(name))
        {
            return true;
        }

        if (name.IndexOf('%') < 0)
        {
            return false;
        }

        try
        {
            return IsSensitiveName(Uri.UnescapeDataString(name));
        }
        catch (UriFormatException)
        {
            return false;
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
            // 既有掩码内的字符不得再被逐字替换（如单字符密钥会把 ****** 逐字展开），按掩码切段后只在段内替换。
            if (text.Contains(Mask))
            {
                var segments = text.Split(Mask);
                for (var index = 0; index < segments.Length; index++)
                {
                    segments[index] = segments[index].Replace(secret, Mask, StringComparison.Ordinal);
                }

                text = string.Join(Mask, segments);
            }
            else
            {
                text = text.Replace(secret, Mask, StringComparison.Ordinal);
            }
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
                    else if (pair.Value is JsonValue scalarValue)
                    {
                        // 数字/布尔等非字符串标量也可能被其他字段回显，按原始 JSON 文本收集。
                        secrets.Add(scalarValue.ToJsonString());
                    }
                    else if (pair.Value != null)
                    {
                        CollectSecretValues(pair.Value, secrets, depth);
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

    /// <summary>
    /// 敏感键下的对象/数组值在被整体掩码前，递归收集其中全部字符串标量（含非字符串标量原始文本），
    /// 供跨字段回显全局替换；深度沿用与 <see cref="RedactText"/> 相同的上限思路，超深即停止收集。
    /// </summary>
    private static void CollectSecretValues(JsonNode node, HashSet<string> secrets, int depth)
    {
        if (depth >= 32)
        {
            return;
        }

        if (node is JsonValue scalar)
        {
            if (scalar.TryGetValue<string>(out var secret) && secret.Length > 0)
            {
                secrets.Add(secret);
            }
            else
            {
                var raw = scalar.ToJsonString();
                if (raw.Length > 0 && raw != Mask)
                {
                    secrets.Add(raw);
                }
            }

            return;
        }

        if (node is JsonObject dictionary)
        {
            foreach (var pair in dictionary)
            {
                if (pair.Value != null)
                {
                    CollectSecretValues(pair.Value, secrets, depth + 1);
                }
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                if (item != null)
                {
                    CollectSecretValues(item, secrets, depth + 1);
                }
            }
        }
    }
}
