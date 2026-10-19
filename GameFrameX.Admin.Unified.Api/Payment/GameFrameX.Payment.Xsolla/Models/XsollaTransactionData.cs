using GameFrameX.Payment.Abstract.Interface;

namespace GameFrameX.Payment.Xsolla.Models;

/// <summary>
/// Xsolla交易数据
/// </summary>
public class XsollaTransactionData : ITransactionData
{
    /// <summary>
    /// 交易ID
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// 外部订单ID
    /// </summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>
    /// 用户ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 交易状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 交易金额
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 货币代码
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// 商品SKU
    /// </summary>
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// 交易时间
    /// </summary>
    public DateTime TransactionTime { get; set; }

    /// <summary>
    /// 是否为沙盒环境
    /// </summary>
    public bool IsSandbox { get; set; }
}