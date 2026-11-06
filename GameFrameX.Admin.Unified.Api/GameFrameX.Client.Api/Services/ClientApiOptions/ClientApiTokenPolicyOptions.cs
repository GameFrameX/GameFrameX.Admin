namespace GameFrameX.Client.Api.Services.ClientApiOptions;

/// <summary>
/// 租户 Client API 访问令牌策略。SigningKey 为 null 表示该租户尚未配置签名密钥。
/// </summary>
public sealed class ClientApiTokenPolicyOptions
{
    /// <summary>
    /// 访问令牌 HMAC 签名密钥；null 表示未配置。
    /// </summary>
    public string? SigningKey { get; init; }

    /// <summary>
    /// 访问令牌有效期（分钟），已受平台上限钳制。
    /// </summary>
    public int LifetimeMinutes { get; init; }
}