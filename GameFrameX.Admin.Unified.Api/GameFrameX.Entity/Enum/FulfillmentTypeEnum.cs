namespace GameFrameX.Entity.Enum;

/// <summary>
/// 权益发放类型。权益发货账本（TenantGameOrderFulfillmentEntity）的发放动作分类。
/// </summary>
[Description("权益发放类型")]
[AdminEnum]
public enum FulfillmentTypeEnum
{
    /// <summary>
    /// 购买发货：支付成功后由支付回调触发的权益发放。
    /// </summary>
    [Description("购买发货")] Purchase = 1,

    /// <summary>
    /// 退款扣除：退款后对已发放权益的回收扣减。
    /// </summary>
    [Description("退款扣除")] Refund = 2,

    /// <summary>
    /// 人工补偿/重发：高风险人工操作，必须记录 OperatorId 审计。
    /// </summary>
    [Description("人工补偿")] Compensate = 3,
}