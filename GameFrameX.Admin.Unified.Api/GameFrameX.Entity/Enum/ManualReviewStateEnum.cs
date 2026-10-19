namespace GameFrameX.Entity.Enum;

/// <summary>
/// 人工复核状态（反作弊事件账本 TenantGameAntiCheatEventEntity.ManualReviewState）。
/// <para>严格锁定 4 值：状态机单向迁移 Pending → Approved/Rejected/Escalated；非 Pending 拒绝重审。
/// 这是"风险 Provider 上报不能绕过人工审核策略"的闸门——Submit 一律写 Pending，SuggestedAction 永不跳过此状态。</para>
/// </summary>
[Description("人工复核状态")]
[AdminEnum]
public enum ManualReviewStateEnum
{
    /// <summary>
    /// 待复核（Submit 默认值）。
    /// </summary>
    [Description("待复核")] Pending = 1,

    /// <summary>
    /// 复核通过（采纳建议；后续 followup 可触发处罚创建，本期只落状态）。
    /// </summary>
    [Description("复核通过")] Approved = 2,

    /// <summary>
    /// 复核驳回（不采纳建议）。
    /// </summary>
    [Description("复核驳回")] Rejected = 3,

    /// <summary>
    /// 升级处理（需更高权限人工介入）。
    /// </summary>
    [Description("升级处理")] Escalated = 4,
}