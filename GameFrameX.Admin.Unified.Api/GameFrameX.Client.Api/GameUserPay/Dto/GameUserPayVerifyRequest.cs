namespace GameFrameX.Client.Api.GameUserPay.Dto;

/// <summary>
/// 支付验真请求（C11 支付域 Verify；凭证按 PayChannel：Apple=ReceiptData 或 TransactionId 二选一 / Google=PurchaseToken）
/// <para>C271 Apple 双形态：StoreKit 1 客户端上报 ReceiptData+TransactionId 走 verifyReceipt 旧路径；
/// StoreKit 2 客户端仅上报 TransactionId 走 App Store Server API v2 新路径（双轨向后兼容，老字段不删不改名）。</para>
/// </summary>
public class GameUserPayVerifyRequest
{
    /// <summary>
    /// 玩家 id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 对外订单号
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// 支付渠道（旧协议渠道号：100=Apple / 7=Google / 66=Amazon）
    /// </summary>
    public int PayChannel { get; set; }

    /// <summary>
    /// Apple 收据数据（Base64；PayChannel=100 时与 TransactionId 二选一：StoreKit 1 上报本字段走 verifyReceipt 旧路径）
    /// </summary>
    public string? ReceiptData { get; set; }

    /// <summary>
    /// Google 购买令牌（PayChannel=7 时必填）
    /// </summary>
    public string? PurchaseToken { get; set; }

    /// <summary>
    /// 渠道交易号（Apple：StoreKit 1 必填 / StoreKit 2 仅凭本字段走 Server API v2 新路径；Google orderId）
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// 渠道商品 ID
    /// </summary>
    public string? ProductId { get; set; }
}