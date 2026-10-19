using System.Text.Json;

namespace GameFrameX.Core.Extension.Json;

/// <summary>
/// System.Text.Json 序列化器实现。
/// </summary>
public sealed class SystemTextJsonProjectSerializer : IProjectJsonSerializer
{
    private readonly JsonSerializerOptions _defaultOptions;

    /// <summary>
    /// 创建使用 Admin 默认 JSON 契约的序列化器。
    /// </summary>
    public SystemTextJsonProjectSerializer()
    {
        _defaultOptions = ProjectJsonSerializerOptions.CreateDefault();
    }

    /// <inheritdoc />
    public string Serialize(object value, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(value, options ?? _defaultOptions);
    }

    /// <inheritdoc />
    public T? Deserialize<T>(string json, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<T>(json, options ?? _defaultOptions);
    }

    /// <inheritdoc />
    public object? Deserialize(string json, Type returnType, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize(json, returnType, options ?? _defaultOptions);
    }

    /// <inheritdoc />
    public JsonSerializerOptions GetSerializerOptions()
    {
        return _defaultOptions;
    }
}