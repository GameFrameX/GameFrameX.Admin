namespace GameFrameX.Client.Api.Integrations.Payment.Dto.Google;

/// <summary>
/// 作废购买的通知
/// </summary>
public class GoogleWebHookInputVoidedPurchaseNotification
{
    /// <summary>
    /// 购买令牌，用于验证购买
    /// </summary>
    public string PurchaseToken { get; set; } = null!;

    /// <summary>
    /// 订单ID
    /// </summary>
    public string OrderId { get; set; } = null!;

    /// <summary>
    /// 商品SKU标识
    /// </summary>
    public string ProductId { get; set; } = null!;

    /// <summary>
    /// 作废时间
    /// </summary>
    public long VoidedTimeMillis { get; set; }

    /// <summary>
    /// 作废原因
    /// 0: Other
    /// 1: Remorse
    /// 2: Not_received
    /// 3: Defective
    /// 4: Accidental_purchase
    /// 5: Fraud
    /// 6: Friendly_fraud
    /// 7: Chargeback
    /// </summary>
    public int VoidedReason { get; set; }

    /// <summary>
    /// 作废来源
    /// 0: User
    /// 1: Developer
    /// 2: Google
    /// </summary>
    public int VoidedSource { get; set; }
}