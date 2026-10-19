using GameFrameX.Client.Api.GameUserPay.Dto;

namespace GameFrameX.Client.Api.Services.PayVerification;

/// <summary>
/// 支付收据验真器（C11 支付域 Verify 通道）
/// <para>默认实现经 <c>IPaymentChannelFactory</c> 按渠道取 handler 调其 VerifyReceiptAsync；
/// 测试注入假实现（成功 / pending / 未配置三态）。</para>
/// </summary>
public interface IPayReceiptVerifier
{
    /// <summary>
    /// 验真一笔订单的渠道凭证。
    /// </summary>
    /// <param name="order">待验真订单（未到账状态）</param>
    /// <param name="request">验真请求（含渠道凭证）</param>
    /// <returns>验真结果三态（成功 / pending 可疑 / 拒绝）</returns>
    Task<GamePayVerifyResponse> VerifyAsync(TenantGameOrderEntity order, GameUserPayVerifyRequest request);
}