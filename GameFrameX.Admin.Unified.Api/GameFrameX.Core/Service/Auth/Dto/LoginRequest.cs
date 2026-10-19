namespace GameFrameX.Core.Service.Auth.Dto;

/// <summary>
/// 用户登录参数
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// 账号
    /// </summary>
    /// <example>admin</example>
    [Required(ErrorMessage = "账号不能为空")]
    [MinLength(2, ErrorMessage = "账号不能少于2个字符")]
    public string Account { get; set; } = null!;

    /// <summary>
    /// 密码
    /// </summary>
    /// <example>123456</example>
    [Required(ErrorMessage = "密码不能为空")]
    [MinLength(6, ErrorMessage = "密码不能少于6个字符")]
    public string Password { get; set; } = null!;

    /// <summary>
    /// 验证码Id
    /// </summary>
    public long CodeId { get; set; }

    /// <summary>
    /// 验证码
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 租户Id。普通租户登录时使用，平台 SuperAdmin 可不传。
    /// </summary>
    public long? TenantId { get; set; }

    /// <summary>
    /// 租户主机名。普通租户登录时可用 Host 解析租户。
    /// </summary>
    public string TenantHost { get; set; } = null!;
}