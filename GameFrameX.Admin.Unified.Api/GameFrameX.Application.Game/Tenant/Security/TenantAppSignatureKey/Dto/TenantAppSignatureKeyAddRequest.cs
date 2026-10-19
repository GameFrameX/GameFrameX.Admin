namespace GameFrameX.Application.Game.Tenant.Security.TenantAppSignatureKey.Dto;

/// <summary>
/// 新增 App 传输签名密钥输入参数。
/// <para>密钥明文由服务端随机生成（不允许外部提交，避免弱密钥入环），响应中一次性回显。</para>
/// </summary>
public class TenantAppSignatureKeyAddRequest
{
    /// <summary>
    /// 应用 ID（密钥域归属）。
    /// </summary>
    [Required(ErrorMessage = "应用 ID 不能为空")]
    public long AppId { get; set; }

    /// <summary>
    /// 生效时间（Unix 秒）；null 表示立即生效。
    /// </summary>
    public long? NotBeforeTime { get; set; }

    /// <summary>
    /// 过期时间（Unix 秒）；null 表示永不过期。
    /// </summary>
    public long? ExpireTime { get; set; }

    /// <summary>
    /// 备注。
    /// </summary>
    public string? Remark { get; set; }
}