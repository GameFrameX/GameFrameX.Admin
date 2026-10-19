namespace GameFrameX.Application.Game.Tenant.Security.TenantAppSignatureKey.Dto;

/// <summary>
/// 续租 App 传输签名密钥输入参数：仅更新到期时间，不产生新密钥。
/// </summary>
public class TenantAppSignatureKeyRenewRequest
{
    /// <summary>
    /// 密钥主键 ID。
    /// </summary>
    [Required(ErrorMessage = "密钥 ID 不能为空")]
    public long Id { get; set; }

    /// <summary>
    /// 新的到期时间（Unix 秒）；null 表示改为永不过期。必须晚于当前时间。
    /// </summary>
    public long? ExpireTime { get; set; }
}