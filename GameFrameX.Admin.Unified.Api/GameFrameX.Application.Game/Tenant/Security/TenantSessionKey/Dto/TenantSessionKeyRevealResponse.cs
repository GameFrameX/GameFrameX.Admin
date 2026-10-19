namespace GameFrameX.Application.Game.Tenant.Security.TenantSessionKey.Dto;

/// <summary>
/// App 会话密钥一次性明文回显输出（仅 AddAsync / RotateAsync 响应使用）。
/// <para>明文只在密钥生成的那一次响应中出现，前端展示后即失；此后任何接口不再提供明文
/// （列表/详情均为 ****** 脱敏，实体禁删）。丢失明文只能通过 RotateAsync 再生成新 key。</para>
/// </summary>
public class TenantSessionKeyRevealResponse
{
    /// <summary>
    /// 主键 ID。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用 ID。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// Key ID，信封 JSON kid 字段。
    /// </summary>
    public string Kid { get; set; } = null!;

    /// <summary>
    /// 会话密钥明文（64 位 hex，仅本次响应回显，请立即复制到客户端注入通道 / SessionKeyRing.SetSessionKey；不再二次提供）。
    /// </summary>
    public string Secret { get; set; } = null!;

    /// <summary>
    /// 生效时间；null 表示无生效下限。
    /// </summary>
    public long? NotBeforeTime { get; set; }

    /// <summary>
    /// 过期时间；null 表示永不过期。
    /// </summary>
    public long? ExpireTime { get; set; }
}