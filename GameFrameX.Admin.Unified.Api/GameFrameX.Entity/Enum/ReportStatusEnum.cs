namespace GameFrameX.Entity.Enum;

/// <summary>
/// 举报状态。举报事实源（TenantGameReportEntity）的生命周期状态。
/// </summary>
[Description("举报状态")]
[AdminEnum]
public enum ReportStatusEnum
{
    /// <summary>
    /// 待处理。
    /// </summary>
    [Description("待处理")] Pending = 1,

    /// <summary>
    /// 调查中。
    /// </summary>
    [Description("调查中")] Investigating = 2,

    /// <summary>
    /// 已坐实：可关联 ResultPenaltyId。
    /// </summary>
    [Description("已坐实")] Confirmed = 3,

    /// <summary>
    /// 已驳回。
    /// </summary>
    [Description("已驳回")] Dismissed = 4,
}