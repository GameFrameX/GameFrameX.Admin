// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using GameFrameX.Payment.Abstract.Models;

namespace GameFrameX.Payment.Abstract;

/// <summary>
/// 渠道退款管理器统一契约（C310 / GFX-718，恢复并升级自 7c23840c 之前的 IRefundManager）。
/// <para>所有支付渠道（Apple / Google / Xsolla / 支付宝）必须提供实现；官方不支持商家主动退款的渠道
/// （Apple：退款由用户发起、App Store 裁决；Google：Android Publisher API 无主动退款端点）
/// 以 <see cref="SupportsProactiveRefund"/> = false 显式声明，调用方据此走统一错误路径，而不是静默缺失。</para>
/// <para>渠道集成层不适用「零引用即删」原则（2026-08-23 B佬决策，教训见 gfx-doc/lessons）。</para>
/// </summary>
public interface IRefundManager
{
    /// <summary>
    /// 支付渠道名称（与 <see cref="PaymentChannelAttribute"/> / IPaymentChannelHandler.ChannelName 对齐；路由大小写不敏感）。
    /// </summary>
    string ChannelName { get; }

    /// <summary>
    /// 是否支持商家主动发起退款（xsolla / alipay = true；apple / google = false，官方政策或 API 不支持）。
    /// </summary>
    bool SupportsProactiveRefund { get; }

    /// <summary>
    /// 发起渠道退款（全额）。不支持主动退款的渠道返回显式失败结果，不抛异常。
    /// </summary>
    /// <param name="context">渠道退款入参聚合（渠道交易号 / 商户订单号 / 金额 / 说明 / 沙箱标记）</param>
    /// <param name="optionSetting">渠道支付配置</param>
    /// <param name="httpClient">HTTP 客户端</param>
    /// <returns>退款发起结果（含渠道原始响应，供审计留档）</returns>
    Task<PaymentChannelRefundResponse> RefundAsync(RefundOrderContext context, IPaymentOptionSetting optionSetting, HttpClient httpClient);

    /// <summary>
    /// 查询渠道侧退款状态（归一为 <see cref="RefundChannelStatusEnum"/>）。
    /// </summary>
    /// <param name="context">渠道退款入参聚合</param>
    /// <param name="optionSetting">渠道支付配置</param>
    /// <param name="httpClient">HTTP 客户端</param>
    /// <returns>退款状态查询结果</returns>
    Task<PaymentChannelRefundQueryResponse> QueryRefundAsync(RefundOrderContext context, IPaymentOptionSetting optionSetting, HttpClient httpClient);

    /// <summary>
    /// 发送消费信息（Apple 专用：用户申请退款时向 App Store 上报消耗信息辅助裁决；其余渠道返回显式失败结果）。
    /// </summary>
    /// <param name="context">渠道退款入参聚合</param>
    /// <param name="optionSetting">渠道支付配置</param>
    /// <param name="httpClient">HTTP 客户端</param>
    /// <returns>消费信息上报结果</returns>
    Task<PaymentChannelRefundResponse> SendConsumptionInformationAsync(RefundOrderContext context, IPaymentOptionSetting optionSetting, HttpClient httpClient);
}
