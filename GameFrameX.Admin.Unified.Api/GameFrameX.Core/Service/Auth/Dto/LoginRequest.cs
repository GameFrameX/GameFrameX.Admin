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

}
