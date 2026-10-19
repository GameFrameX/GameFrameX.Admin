namespace GameFrameX.Entity.Enum;

/// <summary>
/// 申诉状态。申诉事实源（TenantGamePenaltyAppealEntity）的生命周期状态。
/// </summary>
[Description("申诉状态")]
[AdminEnum]
public enum AppealStatusEnum
{
    /// <summary>
    /// 待审核。
    /// </summary>
    [Description("待审核")] Pending = 1,

    /// <summary>
    /// 审核通过：同步撤销关联处罚，写 Revoke* 审计 + SourceAppealId。
    /// </summary>
    [Description("审核通过")] Approved = 2,

    /// <summary>
    /// 审核驳回。
    /// </summary>
    [Description("审核驳回")] Rejected = 3,
}