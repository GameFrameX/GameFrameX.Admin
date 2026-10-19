namespace GameFrameX.Core.Extension.Json;

/// <summary>
/// 项目 JSON 序列化入口。
/// </summary>
public static class ProjectJsonSerializer
{
    /// <summary>
    /// 无 DI 上下文时使用的默认序列化器。
    /// </summary>
    public static IProjectJsonSerializer Default { get; } = new SystemTextJsonProjectSerializer();
}