namespace GameFrameX.Core.Service.PlatformLogging;

/// <summary>
/// 文件日志参数。文件日志注册与文件名模板为结构性参数，随 Host 重启生效。
/// </summary>
public sealed class PlatformLoggingFileSettings
{
    /// <summary>
    /// 是否启用文件日志。默认开启，等价旧 Logging.json 安全默认。
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 日志文件名模板（含 {0:yyyyMMdd}_{1} 占位）。重启生效。
    /// </summary>
    public string FileName { get; set; } = "data/logs/{0:yyyyMMdd}_{1}.log";

    /// <summary>
    /// 是否追加写入。重启生效。
    /// </summary>
    public bool Append { get; set; } = true;

    /// <summary>
    /// 单文件大小上限字节。重启生效。
    /// </summary>
    public long FileSizeLimitBytes { get; set; } = 10485760;

    /// <summary>
    /// 保留的滚动文件数。重启生效。
    /// </summary>
    public int MaxRollingFiles { get; set; } = 30;
}