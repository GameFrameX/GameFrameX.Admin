namespace GameFrameX.Core.Service.PlatformLogging;

/// <summary>
/// 监控日志采集参数。结构性参数，随 Host 重启生效。
/// </summary>
public sealed class PlatformLoggingMonitorSettings
{
    /// <summary>
    /// 是否启用全局监控拦截。重启生效。
    /// </summary>
    public bool GlobalEnabled { get; set; } = true;

    /// <summary>
    /// BusinessError.Oh / Oops.Bah 业务日志输出级别（LogLevel 名称）。重启生效。
    /// </summary>
    public string BahLogLevel { get; set; } = "Information";

    /// <summary>
    /// 是否包含返回值。重启生效。
    /// </summary>
    public bool WithReturnValue { get; set; } = true;

    /// <summary>
    /// 返回值字符串阈值（0=全量）。重启生效。
    /// </summary>
    public int ReturnValueThreshold { get; set; } = 500;

    /// <summary>
    /// JSON 输出行为（None / OnlyJson）。重启生效。
    /// </summary>
    public string JsonBehavior { get; set; } = "OnlyJson";

    /// <summary>
    /// 是否格式化 JSON。重启生效。
    /// </summary>
    public bool JsonIndented { get; set; } = false;

    /// <summary>
    /// 是否使用 UTC 时间戳。重启生效。
    /// </summary>
    public bool UseUtcTimestamp { get; set; } = false;
}