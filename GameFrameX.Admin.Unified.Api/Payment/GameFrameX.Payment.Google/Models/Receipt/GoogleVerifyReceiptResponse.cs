// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using GameFrameX.Payment.Abstract.Interface;
using Google.Apis.AndroidPublisher.v3.Data;

namespace GameFrameX.Payment.Google.Models.Receipt;

/// <summary>
/// Google Play 支付收据验证结果。
/// 继承 <see cref="ProductPurchase"/> 以复用 Google API 返回的所有字段，
/// 同时实现 <see cref="IPaymentVerifyReceiptResponse"/> 接口以适配统一支付抽象。
/// </summary>
public class GoogleVerifyReceiptResponse : ProductPurchase, IPaymentVerifyReceiptResponse
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
    /// 支付渠道交易号
    /// </summary>
    public string ChannelTransactionId
    {
        get { return OrderId ?? string.Empty; }
        set { OrderId = value; }
    }

    /// <summary>
    /// 购买时间
    /// </summary>
    public DateTime PurchaseTime
    {
        get { return PurchaseTimeMillis.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(PurchaseTimeMillis.Value).DateTime : default; }
    }

    /// <summary>
    /// 是否为沙盒环境
    /// </summary>
    public bool IsSandbox { get; set; }

    /// <summary>
    /// 应用包名
    /// </summary>
    public string PackageName { get; set; } = string.Empty;
}