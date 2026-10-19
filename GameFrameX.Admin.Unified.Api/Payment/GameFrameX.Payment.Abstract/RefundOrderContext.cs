// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Abstract;

/// <summary>
/// 渠道退款入参聚合（C310 / GFX-718）：把订单侧退款所需字段一次传齐，避免各渠道实现各自回查订单表。
/// </summary>
public sealed class RefundOrderContext
{
    /// <summary>
    /// 渠道交易号（Xsolla transaction_id / 支付宝 trade_no / Apple originalTransactionId / Google orderId），由路由服务按渠道挑选。
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// 商户订单号（支付宝 out_trade_no / out_request_no 幂等键来源）。
    /// </summary>
    public string OutTradeNo { get; set; } = string.Empty;

    /// <summary>
    /// 退款金额（订单实付金额，最小货币单位；本期恒全额退款，不做部分退款）。
    /// </summary>
    public uint Amount { get; set; }

    /// <summary>
    /// 币种（ISO 4217，如 CNY / USD；渠道未区分时可为空）。
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// 退款说明（管理员填写的退款原因，映射到各渠道的描述/理由字段）。
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 是否沙箱环境（Apple / Google 沙箱端点选择）。
    /// </summary>
    public bool IsSandbox { get; set; }
}
