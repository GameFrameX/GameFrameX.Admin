namespace GameFrameX.Core.Service.PlatformJwt.Dto;

/// <summary>
/// 平台 JWT 配置更新输入。
/// </summary>
public sealed class PlatformJwtConfigUpdateRequest
{
    /// <summary>
    /// 签名密钥。传入 ****** 表示保留当前密钥。
    /// </summary>
    public string IssuerSigningKey { get; set; } = PlatformJwtConfigConstants.MaskedSigningKey;

    /// <summary>
    /// 签发方。
    /// </summary>
    [Required]
    public string ValidIssuer { get; set; } = string.Empty;

    /// <summary>
    /// 签收方。
    /// </summary>
    [Required]
    public string ValidAudience { get; set; } = string.Empty;

    /// <summary>
    /// 过期时间容错秒数。
    /// </summary>
    [Range(0, 300)]
    public long ClockSkew { get; set; } = 5;
}