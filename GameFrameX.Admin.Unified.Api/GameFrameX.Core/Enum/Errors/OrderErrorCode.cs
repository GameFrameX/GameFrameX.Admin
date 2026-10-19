namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// 游戏交易域订单错误码（段 701，见 <see cref="ErrorSegmentRegistry"/>）。
/// </summary>
public enum OrderErrorCode
{
    /// <summary>
    /// 商品不存在（旧码 30000001，原 300 段与 Admin SystemUser 段冲突，按 ADR 重排归 701）
    /// </summary>
    [ErrorText("商品不存在", "Product not found")]
    WebShopNotFound = 70100001,

    /// <summary>
    /// 网页商城回调查无预创建订单（out_trade_no 未命中订单，疑似伪造回调或 Buy 预创建失败后网关重试）
    /// </summary>
    [ErrorText("回调订单不存在", "Notify order not found")]
    NotifyOrderNotFound = 70100002,

    /// <summary>
    /// 网页商城回调金额与预创建订单不符（少付/多付均拒绝发货，需人工核对失败单）
    /// </summary>
    [ErrorText("支付金额与订单不符", "Notify amount mismatch")]
    NotifyAmountMismatch = 70100003,
}