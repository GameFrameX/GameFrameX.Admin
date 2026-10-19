using GameFrameX.Payment.Abstract.Interface;

namespace GameFrameX.Payment.AliPay;

public class AliPayVerifyReceiptResponse : IPaymentVerifyReceiptResponse
{
    public bool IsSuccess { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string RoleId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string ChannelTransactionId { get; set; } = string.Empty;
    public string MerchantOrderId { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool IsSandbox { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;

    /// <summary>
    /// 支付宝交易号
    /// </summary>
    public string TradeNo { get; set; } = string.Empty;
}