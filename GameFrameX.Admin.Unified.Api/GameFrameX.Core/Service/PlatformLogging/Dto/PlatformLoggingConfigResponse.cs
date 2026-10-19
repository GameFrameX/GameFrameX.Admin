namespace GameFrameX.Core.Service.PlatformLogging.Dto;

/// <summary>
/// 平台日志配置输出。ES <see cref="ElasticSearchServerUris"/> 经掩码处理，不回显明文 URI/凭据。
/// </summary>
public sealed class PlatformLoggingConfigResponse
{
    /// <summary>
    /// 文件日志参数。
    /// </summary>
    public PlatformLoggingFileResponse File { get; set; } = new();

    /// <summary>
    /// 数据库日志参数。
    /// </summary>
    public PlatformLoggingDatabaseResponse Database { get; set; } = new();

    /// <summary>
    /// ElasticSearch 日志参数。
    /// </summary>
    public PlatformLoggingElasticSearchResponse ElasticSearch { get; set; } = new();

    /// <summary>
    /// 监控日志采集参数。
    /// </summary>
    public PlatformLoggingMonitorResponse Monitor { get; set; } = new();

    /// <summary>
    /// 最近更新时间（Unix 秒）。
    /// </summary>
    public long? UpdateTime { get; set; }
}