namespace GameFrameX.Core.Service.PlatformJwt;

/// <summary>
/// 平台 JWT 私有配置载荷。
/// </summary>
public sealed class PlatformJwtSettings
{
    /// <summary>
    /// 是否验证签名密钥。
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;

    /// <summary>
    /// 签名密钥。
    /// </summary>
    public string IssuerSigningKey { get; set; } = string.Empty;

    /// <summary>
    /// 是否验证签发方。
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// 签发方。
    /// </summary>
    public string ValidIssuer { get; set; } = string.Empty;

    /// <summary>
    /// 是否验证签收方。
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// 签收方。
    /// </summary>
    public string ValidAudience { get; set; } = string.Empty;

    /// <summary>
    /// 是否验证生命周期。
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// 过期时间容错秒数。
    /// </summary>
    public long ClockSkew { get; set; } = 5;

    /// <summary>
    /// 签名算法。
    /// </summary>
    public string Algorithm { get; set; } = PlatformJwtConfigConstants.Algorithm;

    /// <summary>
    /// 要求 exp 声明。
    /// </summary>
    public bool RequireExpirationTime { get; set; } = true;

    /// <summary>
    /// 全局令牌版本。每次保存配置时递增，使所有旧版本 AccessToken 和 RefreshToken 立即失效。
    /// </summary>
    public long TokenVersion { get; set; } = 1;
}