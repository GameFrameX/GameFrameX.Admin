using GameFrameX.Payment.Google.Option;

namespace GameFrameX.Client.Api.Services.PayVerification;

/// <summary>
/// Google Play 一次性商品购买查询与确认服务。
/// </summary>
public interface IGooglePlayPurchaseService
{
    /// <summary>
    /// 使用候选 Google 支付配置逐个查询购买记录，返回首个可验证的购买与对应配置。
    /// </summary>
    Task<GooglePlayPurchaseVerificationResponse> VerifyPurchaseAsync(
        IReadOnlyCollection<GooglePaymentOptionSetting> settings,
        string packageName,
        string productId,
        string purchaseToken);

    /// <summary>
    /// 确认 Google Play 一次性商品购买。只有发货成功后才能调用。
    /// </summary>
    Task<bool> AcknowledgePurchaseAsync(
        GooglePaymentOptionSetting setting,
        string packageName,
        string productId,
        string purchaseToken,
        string? developerPayload = null);
}