namespace GameFrameX.Application.ClientDelivery.Security.SessionKey.Dto;

/// <summary>
/// 新增 App 会话密钥输入参数。
/// <para>密钥明文由服务端随机生成（不允许外部提交，避免弱密钥入环），响应中一次性回显。</para>
/// </summary>
public class TenantSessionKeyAddRequest
{
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