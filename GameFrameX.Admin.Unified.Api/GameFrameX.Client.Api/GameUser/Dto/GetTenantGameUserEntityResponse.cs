namespace GameFrameX.Client.Api.GameUser.Dto;

/// <summary>
/// 游戏账号增加输入参数
/// </summary>
public sealed class GetTenantGameUserEntityResponse : EntityBaseId
{
    /// <summary>
    /// 昵称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 账号
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    /// 头像
    /// </summary>
    public string Avatar { get; set; } = null!;

    /// <summary>
    /// 手机
    /// </summary>
    public string Phone { get; set; } = null!;

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 谷歌账号ID
    /// </summary>
    public string GoogleId { get; set; } = null!;

    /// <summary>
    /// 苹果账号ID
    /// </summary>
    public string AppleId { get; set; } = null!;

    /// <summary>
    /// Facebook账号ID
    /// </summary>
    public string FacebookId { get; set; } = null!;

    /// <summary>
    /// Twitter账号ID
    /// </summary>
    public string TwitterId { get; set; } = null!;

    /// <summary>
    /// Discord账号ID
    /// </summary>
    public string DiscordId { get; set; } = null!;

    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// 是否为游客
    /// </summary>
    public bool IsGuest { get; set; }

    /// <summary>
    /// 本次请求是否新建账号（C12 A1：DeviceLogin 按设备找号，未命中即建号）
    /// </summary>
    public bool IsNewRegister { get; set; }

    /// <summary>
    /// 账号状态
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Client API 访问凭证。
    /// </summary>
    public string AccessToken { get; set; } = null!;

    /// <summary>
    /// Client API 访问凭证过期时间。
    /// </summary>
    public long ExpireTime { get; set; }

    /// <summary>
    /// 本次登录 Provider。
    /// </summary>
    public string Provider { get; set; } = null!;

    /// <summary>
    /// 第三方展示扩展资料；不包含第三方 access token 或 refresh token。
    /// </summary>
    public object ExternalProfile { get; set; } = null!;

    /// <summary>
    /// 会话密钥 ID（进入客户端加密信封 JSON 的 kid 字段）。仅登录类响应非空，其余响应一律为 null。
    /// </summary>
    public string? SessionKeyId { get; set; }

    /// <summary>
    /// 会话密钥（32 字节 CSPRNG 的 64 位 hex，AES-256-GCM 信封对称密钥）。仅登录类响应一次性回显，其余响应一律为 null。
    /// </summary>
    public string? SessionKeySecret { get; set; }
}