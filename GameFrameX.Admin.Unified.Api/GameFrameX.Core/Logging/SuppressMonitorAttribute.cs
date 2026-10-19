namespace GameFrameX.Core.Logging;

/// <summary>
/// 标记当前接口不写入监控日志。
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class SuppressMonitorAttribute : Attribute
{
}