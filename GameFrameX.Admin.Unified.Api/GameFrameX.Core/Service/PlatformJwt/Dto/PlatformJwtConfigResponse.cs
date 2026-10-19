namespace GameFrameX.Core.Service.PlatformJwt.Dto;

/// <summary>
/// 平台 JWT 配置输出。
/// </summary>
public sealed class PlatformJwtConfigResponse
{
    /// <summary>
    /// 签名密钥掩码。
    /// </summary>
    public string IssuerSigningKey { get; set; } = PlatformJwtConfigConstants.MaskedSigningKey;

    /// <summary>
    /// 是否已配置签名密钥。
    /// </summary>
    public bool HasSigningKey { get; set; }

    /// <summary>
    /// 签发方。
    /// </summary>
    public string ValidIssuer { get; set; } = string.Empty;

    /// <summary>
    /// 签收方。
    /// </summary>
    public string ValidAudience { get; set; } = string.Empty;

    /// <summary>
    /// 过期时间容错秒数。
    /// </summary>
    public long ClockSkew { get; set; }

    /// <summary>
    /// 签名算法。
    /// </summary>
    public string Algorithm { get; set; } = PlatformJwtConfigConstants.Algorithm;

    /// <summary>
    /// 验证签名密钥。
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;

    /// <summary>
    /// 验证签发方。
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// 验证签收方。
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// 验证生命周期。
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// 当前全局令牌版本。
    /// </summary>
    public long TokenVersion { get; set; }

    /// <summary>
    /// 最近更新时间。
    /// </summary>
    public long? UpdateTime { get; set; }
}