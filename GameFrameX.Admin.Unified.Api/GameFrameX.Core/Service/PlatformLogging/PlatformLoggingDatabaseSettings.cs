namespace GameFrameX.Core.Service.PlatformLogging;

/// <summary>
/// 数据库日志参数。IsEnabled 为同 Host 动态开关（DatabaseLoggingWriter 写路径读快照）。
/// </summary>
public sealed class PlatformLoggingDatabaseSettings
{
    /// <summary>
    /// 是否启用数据库日志。默认开启，等价旧 Logging.json 安全默认。保存后同 Host 即时生效。
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 数据库日志最低级别（LogLevel 名称）。重启生效。
    /// </summary>
    public string MinimumLevel { get; set; } = "Information";
}