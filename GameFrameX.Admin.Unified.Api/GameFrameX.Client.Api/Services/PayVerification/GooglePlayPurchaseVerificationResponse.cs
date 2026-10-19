using Google.Apis.AndroidPublisher.v3.Data;
using GameFrameX.Payment.Google.Option;

namespace GameFrameX.Client.Api.Services.PayVerification;

/// <summary>
/// Google Play 购买验证结果。
/// </summary>
public sealed class GooglePlayPurchaseVerificationResponse
{
    /// <summary>
    /// Google Play 返回的一次性商品购买记录。
    /// </summary>
    public ProductPurchase Purchase { get; init; } = null!;

    /// <summary>
    /// 成功验证购买所使用的 Google 支付配置。
    /// </summary>
    public GooglePaymentOptionSetting Setting { get; init; } = null!;
}