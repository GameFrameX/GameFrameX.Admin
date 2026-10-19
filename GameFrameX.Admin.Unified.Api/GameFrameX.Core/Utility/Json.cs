using GameFrameX.Core.Extension.Json;

namespace GameFrameX.Core.Utility;

/// <summary>
/// 项目 JSON 便捷入口，替代历史 历史框架 JSON 门面。
/// </summary>
public static class Json
{
    /// <summary>
    /// 使用项目默认 JSON 契约序列化对象。
    /// </summary>
    public static string Serialize(object? value)
    {
#pragma warning disable CS8604 // 跨项目接口 IProjectJsonSerializer.Serialize 签名 (object) 未标可空，底层 System.Text.Json 实现接受 null
        return ProjectJsonSerializer.Default.Serialize(value);
#pragma warning restore CS8604
    }

    /// <summary>
    /// 使用项目默认 JSON 契约反序列化对象。
    /// </summary>
    public static T? Deserialize<T>(string json)
    {
        return ProjectJsonSerializer.Default.Deserialize<T>(json);
    }
}