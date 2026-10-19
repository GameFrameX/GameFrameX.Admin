using System.Text.Json;

namespace GameFrameX.Core.Tests.ClientApi;

internal static class ClientApiResultReader
{
    public static JsonElement Read(object result)
    {
        var json = JsonSerializer.Serialize(result);
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.TryGetProperty("Value", out var value))
        {
            return value.Clone();
        }

        return document.RootElement.Clone();
    }

    public static int Code(JsonElement root)
    {
        return GetProperty(root, "Code").GetInt32();
    }

    public static string Message(JsonElement root)
    {
        return GetProperty(root, "Message").GetString() ?? string.Empty;
    }

    public static JsonElement Data(JsonElement root)
    {
        var data = GetProperty(root, "Data");
        if (data.ValueKind == JsonValueKind.String)
        {
            var raw = data.GetString();
            if (!string.IsNullOrWhiteSpace(raw))
            {
                var trimmed = raw.TrimStart();
                if (trimmed.StartsWith('{') || trimmed.StartsWith('['))
                {
                    using var document = JsonDocument.Parse(raw);
                    return document.RootElement.Clone();
                }
            }
        }

        return data;
    }

    private static JsonElement GetProperty(JsonElement root, string name)
    {
        if (root.TryGetProperty(name, out var value))
        {
            return value;
        }

        var camelCase = char.ToLowerInvariant(name[0]) + name[1..];
        return root.GetProperty(camelCase);
    }
}