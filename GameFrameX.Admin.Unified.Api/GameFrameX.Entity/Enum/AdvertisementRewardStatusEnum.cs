namespace GameFrameX.Entity.Enum;

/// <summary>
/// 广告奖励发放状态。
/// <para>事实数据应用作用域，记录广告回调奖励从认领到发放结果的状态流转（C114 / GFX-110）。</para>
/// </summary>
[Description("广告奖励发放状态")]
[AdminEnum]
public enum AdvertisementRewardStatusEnum
{
    /// <summary>
    /// 已认领，待发放。账本已按 IdempotencyKey 占位，游戏服发放调用前或进行中。
    /// </summary>
    [Description("已认领，待发放")] Pending = 1,

    /// <summary>
    /// 发放成功。游戏服返回成功，账本终态，同 IdempotencyKey 重放幂等返回不再重复发放。
    /// </summary>
    [Description("发放成功")] Success = 2,

    /// <summary>
    /// 发放失败。FailureReason 必填；同 IdempotencyKey 可重试，复用同一账本行从 Failed 翻回 Pending/Success。
    /// </summary>
    [Description("发放失败")] Failed = 3,
}