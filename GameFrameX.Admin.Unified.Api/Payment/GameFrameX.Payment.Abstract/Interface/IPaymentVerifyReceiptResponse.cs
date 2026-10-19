// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Abstract.Interface;

/// <summary>
/// 支付收据验证结果接口，定义验证支付收据后返回的关键信息。
/// </summary>
public interface IPaymentVerifyReceiptResponse
{
    /// <summary>
    /// 是否验证成功（如为 true，表示支付收据验证通过；false 表示验证失败或无需支付）。
    /// </summary>
    bool IsSuccess { get; set; }

    /// <summary>
    /// 商品 SKU（库存单位标识，唯一标识本次支付的商品）。
    /// </summary>
    string Sku { get; set; }

    /// <summary>
    /// 角色 ID（下单用户的角色唯一标识，通常用于区分不同用户或角色）。
    /// </summary>
    string RoleId { get; set; }

    /// <summary>
    /// 订单 ID（下单后生成的唯一订单标识，部分支付方式可能不存在）。
    /// </summary>
    string OrderId { get; set; }

    /// <summary>
    /// 支付渠道交易号。成功结果必须提供可用于唯一识别渠道交易的值。
    /// </summary>
    string ChannelTransactionId { get; set; }

    /// <summary>
    /// 商户订单号。成功结果必须绑定到本系统订单。
    /// </summary>
    string MerchantOrderId { get; set; }

    /// <summary>
    /// 账号 ID。用于表达支付结果与下单账号的绑定关系。
    /// </summary>
    string AccountId { get; set; }

    /// <summary>
    /// 支付金额。成功结果必须来自渠道验签或查询后的金额。
    /// </summary>
    decimal Amount { get; set; }

    /// <summary>
    /// 币种代码，如 CNY、USD。
    /// </summary>
    string Currency { get; set; }

    /// <summary>
    /// 是否为沙盒环境。成功结果必须与订单环境一致。
    /// </summary>
    bool IsSandbox { get; set; }

    /// <summary>
    /// 渠道或订单状态。成功结果必须来自已验证的可支付完成状态。
    /// </summary>
    string OrderStatus { get; set; }

    /// <summary>
    /// 幂等键。默认使用渠道交易号或商户订单号，调用方可据此防止重复发货。
    /// </summary>
    string IdempotencyKey { get; set; }
}