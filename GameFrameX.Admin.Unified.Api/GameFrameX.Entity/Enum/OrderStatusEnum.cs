namespace GameFrameX.Entity.Enum;

/// <summary>
/// 订单状态
/// </summary>
[Description("订单状态")]
[AdminEnum]
public enum OrderStatusEnum
{
    /// <summary>
    /// 未付款
    /// </summary>
    [Description("未付款")] Unpaid = 1,

    /// <summary>
    /// 未发货
    /// </summary>
    [Description("未发货")] NotShipped = 2,

    /// <summary>
    /// 已发货
    /// </summary>
    [Description("已发货")] Shipped = 3,

    /// <summary>
    /// 发起退款
    /// </summary>
    [Description("发起退款")] InitiateRefund = 4,

    /// <summary>
    /// 退款拒绝
    /// </summary>
    [Description("退款拒绝")] RefundRefused = 5,

    /// <summary>
    /// 同意退款
    /// </summary>
    [Description("同意退款")] RefundAgreement,

    /// <summary>
    /// 已退款
    /// </summary>
    [Description("已退款")] Refunded,

    /// <summary>
    /// 已完成
    /// </summary>
    [Description("已完成")] Completed,

    /// <summary>
    /// 已作废
    /// </summary>
    [Description("已作废")] Voided,
}