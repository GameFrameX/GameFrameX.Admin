using Microsoft.IdentityModel.Tokens;

namespace GameFrameX.Core.Service.PlatformJwt;

/// <summary>
/// 平台 JWT 配置常量。
/// </summary>
public static class PlatformJwtConfigConstants
{
    /// <summary>
    /// SystemConfigEntity 中保存平台 JWT 私有配置的固定编码。
    /// </summary>
    public const string ConfigCode = "platform_jwt_settings";

    /// <summary>
    /// 密钥掩码。
    /// </summary>
    public const string MaskedSigningKey = "******";

    /// <summary>
    /// 令牌版本 Claim。
    /// </summary>
    public const string TokenVersionClaim = "platform_jwt_version";

    /// <summary>
    /// 支持的唯一签名算法。
    /// </summary>
    public const string Algorithm = SecurityAlgorithms.HmacSha256;
}