namespace GameFrameX.Application.Game.Tenant.Security.TenantAppSignatureKey.Dto;

/// <summary>
/// 轮换 App 传输签名密钥输入参数：新插一把立即生效的 key（双活），旧 key 不动、按其租约自然退场。
/// </summary>
public class TenantAppSignatureKeyRotateRequest
{
    /// <summary>
    /// 应用 ID（密钥域归属）。
    /// </summary>
    [Required(ErrorMessage = "应用 ID 不能为空")]
    public long AppId { get; set; }

    /// <summary>
    /// 旧密钥的新到期时间（Unix 秒）：建议设为「双活窗口」结束时刻（客户端全量切换完成后）；
    /// null 表示不改动旧密钥租约（旧密钥按原到期时间或永久继续双活）。
    /// </summary>
    public long? OldKeyExpireTime { get; set; }

    /// <summary>
    /// 备注。
    /// </summary>
    public string? Remark { get; set; }
}