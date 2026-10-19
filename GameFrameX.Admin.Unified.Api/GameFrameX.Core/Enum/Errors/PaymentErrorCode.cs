namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// 游戏交易域支付错误码（段 704，见 <see cref="ErrorSegmentRegistry"/>）。
/// </summary>
public enum PaymentErrorCode
{
    /// <summary>
    /// 支付渠道不支持
    /// </summary>
    [ErrorText("支付渠道不支持", "Payment channel not supported")]
    PayChannelNotSupported = 70400001,

    /// <summary>
    /// 渠道不支持商家主动发起退款（Apple：退款由用户发起、App Store 裁决；Google：Android Publisher API 无主动退款端点）（C310 / GFX-718）
    /// </summary>
    [ErrorText("该支付渠道不支持商家主动发起退款", "This payment channel does not support merchant-initiated refunds")]
    RefundNotSupported = 70400002,

    /// <summary>
    /// 渠道退款所需的支付配置缺失（租户支付配置表中无该渠道该应用的启用配置）（C310 / GFX-718）
    /// </summary>
    [ErrorText("退款失败：渠道支付配置缺失", "Refund failed: payment channel configuration is missing")]
    RefundConfigMissing = 70400003,

    /// <summary>
    /// 渠道退款接口返回失败（渠道侧拒绝或网络/网关错误，本地账本未扣除）（C310 / GFX-718）
    /// </summary>
    [ErrorText("渠道退款失败", "Payment channel refund failed")]
    RefundChannelFailed = 70400004,
}