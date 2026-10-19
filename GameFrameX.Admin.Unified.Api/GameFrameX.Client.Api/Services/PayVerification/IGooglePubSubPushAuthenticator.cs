using GameFrameX.Payment.Google.Option;

namespace GameFrameX.Client.Api.Services.PayVerification;

/// <summary>
/// 校验 Google Pub/Sub authenticated push 的 OIDC JWT。
/// </summary>
public interface IGooglePubSubPushAuthenticator
{
    /// <summary>
    /// 验证请求头中的 Bearer token 是否由配置的 Pub/Sub push 服务账号签发，且 audience 匹配配置值。
    /// </summary>
    Task<bool> ValidateAsync(string authorizationHeader, IReadOnlyCollection<GooglePaymentOptionSetting> settings);
}