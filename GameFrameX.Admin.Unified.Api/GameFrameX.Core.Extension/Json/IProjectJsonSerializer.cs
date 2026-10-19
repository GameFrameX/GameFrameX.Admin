using System.Text.Json;

namespace GameFrameX.Core.Extension.Json;

/// <summary>
/// 项目自有 JSON 序列化契约。
/// </summary>
public interface IProjectJsonSerializer
{
    /// <summary>
    /// 使用 Admin 统一 JSON 契约序列化对象。
    /// </summary>
    string Serialize(object value, JsonSerializerOptions? options = null);

    /// <summary>
    /// 使用 Admin 统一 JSON 契约反序列化为指定类型。
    /// </summary>
    T? Deserialize<T>(string json, JsonSerializerOptions? options = null);

    /// <summary>
    /// 使用 Admin 统一 JSON 契约反序列化为运行时类型。
    /// </summary>
    object? Deserialize(string json, Type returnType, JsonSerializerOptions? options = null);

    /// <summary>
    /// 获取当前序列化器的默认 JSON 选项。
    /// </summary>
    JsonSerializerOptions GetSerializerOptions();
}