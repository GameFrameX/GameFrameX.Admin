using System.Text.Json;
using SKIT.FlurlHttpClient;

namespace GameFrameX.Wechat.Abstract;

public class SystemTextJsonSerializer : IJsonSerializer
{
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTextJsonSerializer"/> class.
    /// </summary>
    /// <param name="options">Options to control (de)serialization behavior.</param>
    public SystemTextJsonSerializer(JsonSerializerOptions? options = null)
    {
        _options = options ?? new JsonSerializerOptions { PropertyNameCaseInsensitive = true, };
    }

    public string Serialize(object? obj, Type type)
    {
        return JsonSerializer.Serialize(obj, type, _options);
    }

    public object? Deserialize(string json, Type type)
    {
        return JsonSerializer.Deserialize(json, type, _options);
    }
}