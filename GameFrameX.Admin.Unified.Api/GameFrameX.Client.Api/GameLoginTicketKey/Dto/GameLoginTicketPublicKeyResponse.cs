namespace GameFrameX.Client.Api.GameLoginTicketKey.Dto;

/// <summary>
/// Game 登录票据公钥输出。
/// </summary>
public sealed class GameLoginTicketPublicKeyResponse
{
    /// <summary>
    /// 租户 ID。
    /// </summary>
    public long TenantId { get; set; }

    /// <summary>
    /// 应用 ID。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// Key ID。
    /// </summary>
    public string Kid { get; set; } = null!;

    /// <summary>
    /// 签名算法。
    /// </summary>
    public string Algorithm { get; set; } = null!;

    /// <summary>
    /// PEM 格式公钥。
    /// </summary>
    public string PublicKeyPem { get; set; } = null!;

    /// <summary>
    /// 生效时间。
    /// </summary>
    public long? NotBeforeTime { get; set; }

    /// <summary>
    /// 过期时间。
    /// </summary>
    public long? ExpireTime { get; set; }
}