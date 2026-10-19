// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using GameFrameX.Payment.Abstract.Interface;

namespace GameFrameX.Payment.Xsolla.Models;

/// <summary>
/// Xsolla 支付收据验证结果，实现 IPaymentVerifyReceiptResponse 接口
/// </summary>
public class XsollaVerifyReceiptResponse : IPaymentVerifyReceiptResponse
{
    /// <summary>
    /// 是否验证成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 商品 SKU
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// 角色 ID
    /// </summary>
    public string RoleId { get; set; } = string.Empty;

    /// <summary>
    /// 订单 ID
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// Xsolla 交易 ID
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

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
    /// 外部订单 ID
    /// </summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>
    /// 交易金额
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 货币代码
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// 交易状态
    /// </summary>
    public string TransactionStatus { get; set; } = string.Empty;

    /// <summary>
    /// 订单状态
    /// </summary>
    public string OrderStatus { get; set; } = string.Empty;

    /// <summary>
    /// 幂等键
    /// </summary>
    public string IdempotencyKey { get; set; } = string.Empty;

    /// <summary>
    /// 交易时间
    /// </summary>
    public DateTime TransactionTime { get; set; }

    /// <summary>
    /// 是否为沙盒环境
    /// </summary>
    public bool IsSandbox { get; set; }

    /// <summary>
    /// 用户邮箱
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// 购买的商品列表
    /// </summary>
    public List<XsollaVerifyReceiptItem> Items { get; set; } = new();
}