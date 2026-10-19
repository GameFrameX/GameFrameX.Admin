namespace GameFrameX.Application.Game.Tenant.LoginTicket.TenantGameLoginTicketKey.Dto;

/// <summary>
/// 签名密钥手动轮换输入参数。
/// </summary>
public class TenantGameLoginTicketKeyRotateRequest
{
    /// <summary>
    /// 应用 ID。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 新密钥的到期时间；null 表示永不过期。
    /// </summary>
    public long? ExpireTime { get; set; }

    /// <summary>
    /// 旧密钥退场时间（Unix 秒，C373）：轮换后旧钥被打 RotatedTime 标记并收窄到期时间到此值，
    /// 到点被生效查询自然排除（Server 按 kid 验签，窗口内旧公钥仍可验）。
    /// <para>null = 默认 now + 7 天；不得早于当前时间，不得晚于 now + 30 天（对齐票据 30 天上限口径）。</para>
    /// </summary>
    public long? OldKeyExpireTime { get; set; }
}
