using GameFrameX.Payment.Abstract.Interface;

namespace GameFrameX.Payment.Apple.Models;

/// <summary>
/// Apple支付交易数据
/// </summary>
public sealed class AppleTransactionData : ITransactionData
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 商品SKU
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// 订单ID
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// 收据数据（Base64编码）
    /// </summary>
    public string ReceiptData { get; set; } = string.Empty;

    /// <summary>
    /// 交易ID
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// 期望的产品ID（可选，用于验证）
    /// </summary>
    public string? ExpectedProductId { get; set; }

    /// <summary>
    /// 是否为沙盒环境
    /// </summary>
    public bool IsSandbox { get; set; }

    /// <summary>
    /// 原始交易ID（用于订阅产品）
    /// </summary>
    public string? OriginalTransactionId { get; set; }

    /// <summary>
    /// 期望的购买数量
    /// </summary>
    public int? ExpectedQuantity { get; set; }

    /// <summary>
    /// 验证时间戳
    /// </summary>
    public DateTime? VerificationTime { get; set; }
}