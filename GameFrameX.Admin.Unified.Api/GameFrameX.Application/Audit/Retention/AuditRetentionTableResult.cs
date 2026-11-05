namespace GameFrameX.Application.Audit.Retention;

/// <summary>
/// 单张源表的留存处理结果。
/// </summary>
public sealed class AuditRetentionTableResult
{
    /// <summary>
    /// 日志种类标题（访问日志 / 操作日志）。
    /// </summary>
    public string LogKind { get; set; } = string.Empty;

    /// <summary>
    /// 本次从源表搬入归档表的行数。
    /// </summary>
    public long ArchivedCount { get; set; }

    /// <summary>
    /// 本次从归档表按保留天数清理的行数。
    /// </summary>
    public long PurgedCount { get; set; }
}
