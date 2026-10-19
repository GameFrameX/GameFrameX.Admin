using GameFrameX.Payment.Abstract.Interface;

namespace GameFrameX.Payment.Apple.Models.Verification;

/// <summary>
/// Apple支付验证收据结果
/// </summary>
public class AppleVerifyReceiptResponse : IPaymentVerifyReceiptResponse
{
    /// <summary>
    /// 是否验证成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 商品SKU
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// 角色ID
    /// </summary>
    public string RoleId { get; set; } = string.Empty;

    /// <summary>
    /// 订单ID
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// 支付渠道交易号
    /// </summary>
    public string ChannelTransactionId { get; set; } = string.Empty;

    /// <summary>
    /// 商户订单号
    /// </summary>
    public string MerchantOrderId { get; set; } = string.Empty;

    /// <summary>
    /// 账号 ID
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// 支付金额
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 币种代码
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// 订单状态
    /// </summary>
    public string OrderStatus { get; set; } = string.Empty;

    /// <summary>
    /// 幂等键
    /// </summary>
    public string IdempotencyKey { get; set; } = string.Empty;

    /// <summary>
    /// 交易ID
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// 原始交易ID
    /// </summary>
    public string OriginalTransactionId { get; set; } = string.Empty;

    /// <summary>
    /// 产品ID
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// 购买日期
    /// </summary>
    public DateTime PurchaseDate { get; set; }

    /// <summary>
    /// 购买日期（毫秒时间戳）
    /// </summary>
    public long PurchaseDateMs { get; set; }

    /// <summary>
    /// 原始购买日期
    /// </summary>
    public DateTime OriginalPurchaseDate { get; set; }

    /// <summary>
    /// 原始购买日期（毫秒时间戳）
    /// </summary>
    public long OriginalPurchaseDateMs { get; set; }

    /// <summary>
    /// 过期日期（订阅产品）
    /// </summary>
    public DateTime ExpiresDate { get; set; }

    /// <summary>
    /// 过期日期（毫秒时间戳，订阅产品）
    /// </summary>
    public long? ExpiresDateMs { get; set; }

    /// <summary>
    /// 购买数量
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// 应用Bundle ID
    /// </summary>
    public string BundleId { get; set; } = string.Empty;

    /// <summary>
    /// 应用版本
    /// </summary>
    public string ApplicationVersion { get; set; } = string.Empty;

    /// <summary>
    /// 是否为沙盒环境
    /// </summary>
    public bool IsSandbox { get; set; }

    /// <summary>
    /// 验证状态码
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 是否为试用期
    /// </summary>
    public bool IsTrialPeriod { get; set; }

    /// <summary>
    /// 是否在介绍期优惠中
    /// </summary>
    public bool IsInIntroOfferPeriod { get; set; }

    /// <summary>
    /// 取消日期
    /// </summary>
    public DateTime CancellationDate { get; set; }

    /// <summary>
    /// 取消原因
    /// </summary>
    public string CancellationReason { get; set; } = string.Empty;

    /// <summary>
    /// Web订单行项目ID
    /// </summary>
    public string WebOrderLineItemId { get; set; } = string.Empty;

    /// <summary>
    /// 促销优惠ID
    /// </summary>
    public string PromotionalOfferId { get; set; } = string.Empty;

    /// <summary>
    /// 订阅组标识符
    /// </summary>
    public string SubscriptionGroupIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// 环境（Sandbox/Production）
    /// </summary>
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// 收据创建日期
    /// </summary>
    public DateTime ReceiptCreationDate { get; set; }

    /// <summary>
    /// 收据创建日期（毫秒时间戳）
    /// </summary>
    public long ReceiptCreationDateMs { get; set; }

    /// <summary>
    /// 请求日期
    /// </summary>
    public DateTime RequestDate { get; set; }

    /// <summary>
    /// 请求日期（毫秒时间戳）
    /// </summary>
    public long RequestDateMs { get; set; }
}