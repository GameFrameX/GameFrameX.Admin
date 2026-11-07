namespace GameFrameX.Application.Audit.Retention;

/// <summary>
/// 审计留存配置（契约）。默认值在代码中，`Configuration/AuditRetention.json` 可覆盖。
/// </summary>
public sealed class AuditRetentionOptions
{
    /// <summary>
    /// 配置节名。
    /// </summary>
    public const string ConfigurationSectionName = "AuditRetention";

    /// <summary>
    /// 留存作业总开关。关闭时作业空跑，不搬移、不清理任何数据。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 访问日志热数据保留天数，默认 30 天。
    /// </summary>
    public int VisitHotDataDays { get; set; } = 30;

    /// <summary>
    /// 操作日志热数据保留天数，默认 180 天。
    /// </summary>
    public int OperationHotDataDays { get; set; } = 180;

    /// <summary>
    /// 归档表保留天数，0 表示永久保留。
    /// </summary>
    public int ArchiveRetentionDays { get; set; } = 0;

    /// <summary>
    /// 每批处理的行数，默认 5000 行。
    /// </summary>
    public int BatchSize { get; set; } = 5000;
}
