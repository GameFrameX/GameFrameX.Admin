using Google.Apis.Auth;
using GameFrameX.Payment.Google.Option;
using GameFrameX.Client.Api.Integrations.Payment;

namespace GameFrameX.Client.Api.Services.PayVerification;

/// <summary>
/// 使用 Google 官方 JWT 校验器验证 Pub/Sub authenticated push 请求。
/// </summary>
public sealed class GooglePubSubPushAuthenticator : IGooglePubSubPushAuthenticator
{
    private const string BearerPrefix = "Bearer ";

    /// <inheritdoc />
    public async Task<bool> ValidateAsync(string authorizationHeader, IReadOnlyCollection<GooglePaymentOptionSetting> settings)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader) ||
            !authorizationHeader.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var token = authorizationHeader[BearerPrefix.Length..].Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var configuredSettings = settings
                                 .Where(PaymentWebhookPolicy.HasGooglePubSubPushAuthenticationConfig)
                                 .ToList();
        if (configuredSettings.Count == 0)
        {
            return false;
        }

        foreach (var setting in configuredSettings)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(token, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { setting.GooglePaymentPubSubPushAudience, },
                });

                if (payload.EmailVerified == true &&
                    string.Equals(payload.Email, setting.GooglePaymentPubSubPushServiceAccountEmail, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warning($"Google Pub/Sub push JWT 校验失败，继续尝试候选配置: {ex.Message}");
            }
        }

        return false;
    }
}