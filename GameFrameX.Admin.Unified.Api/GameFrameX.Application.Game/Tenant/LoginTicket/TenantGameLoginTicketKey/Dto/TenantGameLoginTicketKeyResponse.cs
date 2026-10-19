namespace GameFrameX.Application.Game.Tenant.LoginTicket.TenantGameLoginTicketKey.Dto;

/// <summary>
/// 租户 Game 登录票据签名密钥输出参数（仅公钥与元数据，私钥永不回显）。
/// </summary>
public class TenantGameLoginTicketKeyResponse
{
    /// <summary>
    /// 主键 ID。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 租户 ID。
    /// </summary>
    public long TenantId { get; set; }

    /// <summary>
    /// 应用 ID。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// Key ID，写入 JWT header 的 kid。
    /// </summary>
    public string Kid { get; set; } = null!;

    /// <summary>
    /// 签名算法。
    /// </summary>
    public string Algorithm { get; set; } = null!;

    /// <summary>
    /// PEM 格式公钥，供 Server 验签。
    /// </summary>
    public string PublicKeyPem { get; set; } = null!;

    /// <summary>
    /// 生效时间。
    /// </summary>
    public long? NotBeforeTime { get; set; }

    /// <summary>
    /// 过期时间；null 表示永不过期。
    /// </summary>
    public long? ExpireTime { get; set; }

    /// <summary>
    /// 轮换时间。
    /// </summary>
    public long? RotatedTime { get; set; }

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 备注。
    /// </summary>
    public string Remark { get; set; } = null!;

    /// <summary>
    /// 创建时间。
    /// </summary>
    public long CreatedTime { get; set; }
}