using GameFrameX.Entity.Enum;

namespace GameFrameX.Wechat.Open.Dto;

/// <summary>
/// 增加微信用户输入参数（仅业务可写字段；租户/审计等系统字段由服务端注入，AccessToken/RefreshToken 凭证禁止经请求写入）
/// </summary>
public class AddWechatUserRequest
{
    /// <summary>
    /// 租户用户Id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 平台类型
    /// </summary>
    public PlatformTypeEnum PlatformType { get; set; } = PlatformTypeEnum.微信公众号;

    /// <summary>
    /// OpenId
    /// </summary>
    public string OpenId { get; set; } = string.Empty;

    /// <summary>
    /// 会话密钥
    /// </summary>
    public string? SessionKey { get; set; }

    /// <summary>
    /// UnionId
    /// </summary>
    public string? UnionId { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string? NickName { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    public string? Avatar { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    public string? Mobile { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public int? Sex { get; set; }

    /// <summary>
    /// 语言
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// 城市
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// 省
    /// </summary>
    public string? Province { get; set; }

    /// <summary>
    /// 国家
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public int? ExpiresIn { get; set; }

    /// <summary>
    /// 用户授权的作用域，使用逗号分隔
    /// </summary>
    public string? Scope { get; set; }
}
