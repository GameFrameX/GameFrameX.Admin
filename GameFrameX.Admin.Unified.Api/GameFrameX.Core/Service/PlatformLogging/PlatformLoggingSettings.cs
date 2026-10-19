namespace GameFrameX.Core.Service.PlatformLogging;

/// <summary>
/// 平台日志私有配置载荷。字段严格等于 change C171 §3 列举集合，禁止未列字段。
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class PlatformLoggingSettings
{
    /// <summary>
    /// 文件日志参数。
    /// </summary>
    public PlatformLoggingFileSettings File { get; set; } = new();

    /// <summary>
    /// 数据库日志参数。
    /// </summary>
    public PlatformLoggingDatabaseSettings Database { get; set; } = new();

    /// <summary>
    /// ElasticSearch 日志参数。
    /// </summary>
    public PlatformLoggingElasticSearchSettings ElasticSearch { get; set; } = new();

    /// <summary>
    /// 监控日志采集参数。
    /// </summary>
    public PlatformLoggingMonitorSettings Monitor { get; set; } = new();
}