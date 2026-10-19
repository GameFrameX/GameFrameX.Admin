using GameFrameX.Core.Service;

namespace GameFrameX.Application.Game.Tenant.Security.TenantAppSignatureKey.Dto;

/// <summary>
/// App 传输签名密钥输出参数（列表/详情，密钥脱敏）。
/// <para><see cref="Secret"/> 为固定脱敏占位值 ******：HMAC 对称密钥验签必需明文落库（见实体注释），
/// 但任何列表/详情查询都不得回显明文；明文仅在 AddAsync/RotateAsync 响应（<see cref="TenantAppSignatureKeyRevealResponse"/>）一次性回显。</para>
/// </summary>
public class TenantAppSignatureKeyResponse
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
    /// Key ID，v2 签名头与 message 串中的 keyId。
    /// </summary>
    public string Kid { get; set; } = null!;

    /// <summary>
    /// 密钥脱敏占位（固定 ******），真实明文仅 AddAsync/RotateAsync 一次性回显。
    /// </summary>
    public string Secret { get; set; } = SensitiveFieldMask.MaskValue;

    /// <summary>
    /// 生效时间；null 表示无生效下限。
    /// </summary>
    public long? NotBeforeTime { get; set; }

    /// <summary>
    /// 过期时间；null 表示永不过期。
    /// </summary>
    public long? ExpireTime { get; set; }

    /// <summary>
    /// 被轮换时间；null 表示从未被轮换。
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