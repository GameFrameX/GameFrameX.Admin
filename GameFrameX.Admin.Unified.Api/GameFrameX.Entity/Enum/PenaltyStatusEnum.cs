namespace GameFrameX.Entity.Enum;

/// <summary>
/// 处罚状态。处罚事实源（TenantGamePenaltyEntity）的生命周期状态。
/// </summary>
[Description("处罚状态")]
[AdminEnum]
public enum PenaltyStatusEnum
{
    /// <summary>
    /// 生效中：Enforcement 命中候选（配合 ExpireTime &gt; now 读取时过期判定）。
    /// </summary>
    [Description("生效中")] Active = 1,

    /// <summary>
    /// 已撤销：人工 / 申诉通过解除，Revoke* 审计必填。
    /// </summary>
    [Description("已撤销")] Revoked = 2,

    /// <summary>
    /// 已过期：保留枚举值；本期无定时任务写入，过期由 Enforcement 读取时 ExpireTime &gt; now 判定，Status 仍为 Active。
    /// </summary>
    [Description("已过期")] Expired = 3,
}