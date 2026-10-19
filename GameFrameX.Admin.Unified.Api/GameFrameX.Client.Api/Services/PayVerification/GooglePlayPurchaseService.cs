using GameFrameX.Payment.Google;
using GameFrameX.Payment.Google.Option;

namespace GameFrameX.Client.Api.Services.PayVerification;

/// <summary>
/// 通过 Android Publisher API 查询并确认 Google Play 购买。
/// </summary>
public sealed class GooglePlayPurchaseService : IGooglePlayPurchaseService
{
    /// <inheritdoc />
    public async Task<GooglePlayPurchaseVerificationResponse> VerifyPurchaseAsync(
        IReadOnlyCollection<GooglePaymentOptionSetting> settings,
        string packageName,
        string productId,
        string purchaseToken)
    {
        foreach (var setting in settings)
        {
            try
            {
                await GooglePlayApiManager.InitializeAsync(setting);
                var resolvedPackageName = string.IsNullOrWhiteSpace(packageName) ? setting.GooglePaymentPackageName : packageName;
                var purchase = await GooglePlayApiManager.VerifyPurchaseAsync(resolvedPackageName, productId, purchaseToken);
                if (purchase != null)
                {
                    return new GooglePlayPurchaseVerificationResponse
                    {
                        Purchase = purchase,
                        Setting = setting,
                    };
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warning($"Google支付通知验证失败，继续尝试候选配置: {ex.Message}");
            }
        }

#pragma warning disable CS8603 // 接口 IGooglePlayPurchaseService 签名非空（跨项目共享，测试 fake 实现），未命中时返回 null 是契约语义
        return null;
#pragma warning restore CS8603
    }

    /// <inheritdoc />
    public async Task<bool> AcknowledgePurchaseAsync(
        GooglePaymentOptionSetting setting,
        string packageName,
        string productId,
        string purchaseToken,
        string? developerPayload = null)
    {
        if (setting == null)
        {
            return false;
        }

        await GooglePlayApiManager.InitializeAsync(setting);
        var resolvedPackageName = string.IsNullOrWhiteSpace(packageName) ? setting.GooglePaymentPackageName : packageName;
        return await GooglePlayApiManager.AcknowledgePurchaseAsync(resolvedPackageName, productId, purchaseToken, developerPayload);
    }
}