namespace GameFrameX.Application.Audit.Retention;

/// <summary>
/// 一次留存执行的汇总结果。
/// </summary>
public sealed class AuditRetentionExecutionResult
{
    /// <summary>
    /// 留存开关是否开启（false 时本次为空跑，未搬移任何数据）。
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 各源表处理结果。
    /// </summary>
    public List<AuditRetentionTableResult> Tables { get; set; } = new();

    /// <summary>
    /// 总耗时（毫秒）。
    /// </summary>
    public long ElapsedMilliseconds { get; set; }
}
