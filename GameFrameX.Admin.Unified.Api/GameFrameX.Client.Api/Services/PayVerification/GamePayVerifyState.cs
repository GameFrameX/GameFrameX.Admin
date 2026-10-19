namespace GameFrameX.Client.Api.Services.PayVerification;

/// <summary>
/// 验真结果三态（C11 支付域）
/// </summary>
public enum GamePayVerifyState
{
    /// <summary>
    /// 验真成功（渠道确认到账，订单转 Shipped）
    /// </summary>
    Succeeded = 0,

    /// <summary>
    /// pending / 可疑（不记账，Rcode=7，客户端可重试）
    /// </summary>
    Pending = 1,

    /// <summary>
    /// 拒绝（渠道不支持 / 支付配置未加载 / 必要凭证缺失；明确错误码返回，绝不静默放行）
    /// </summary>
    Rejected = 2,
}