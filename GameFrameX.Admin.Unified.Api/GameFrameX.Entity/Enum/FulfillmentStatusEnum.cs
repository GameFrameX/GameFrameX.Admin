namespace GameFrameX.Entity.Enum;

/// <summary>
/// 权益发放状态。权益发货账本（TenantGameOrderFulfillmentEntity）的单笔发放生命周期状态。
/// 与订单 OrderStatusEnum 独立：账本记录「权益实际发放结果」，订单记录「订单交易状态」。
/// </summary>
[Description("权益发放状态")]
[AdminEnum]
public enum FulfillmentStatusEnum
{
    /// <summary>
    /// 权益发放意图：支付成功已产生，尚未实际发货。当前实现中发货为同步链路，意图态短暂，主要保留用于异步发货演进。
    /// </summary>
    [Description("发放意图")] Pending = 1,

    /// <summary>
    /// 实际发放成功（发货/退款扣除/人工补偿成功）。
    /// </summary>
    [Description("发放成功")] Success = 2,

    /// <summary>
    /// 发放失败。FailureReason 必填，记录失败原因供重试与审计。
    /// </summary>
    [Description("发放失败")] Failed = 3,

    /// <summary>
    /// 作废：订单未实际发货即退款/作废，无实际权益发放或扣除。
    /// </summary>
    [Description("已作废")] Voided = 4,
}