namespace GameFrameX.Core.Service.Auth.Dto;

/// <summary>
/// 邮箱自助注册参数
/// </summary>
public class RegisterByEmailRequest
{
    /// <summary>
    /// 注册邮箱（同时作为租管账号）
    /// </summary>
    /// <example>admin@example.com</example>
    [Required(ErrorMessage = "邮箱不能为空")]
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string Email { get; set; } = null!;

    /// <summary>
    /// 邮箱验证码
    /// </summary>
    /// <example>123456</example>
    [Required(ErrorMessage = "验证码不能为空")]
    [MinLength(6, ErrorMessage = "验证码不能少于6个字符")]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 登录密码
    /// </summary>
    /// <example>123456</example>
    [Required(ErrorMessage = "密码不能为空")]
    [MinLength(6, ErrorMessage = "密码不能少于6个字符")]
    public string Password { get; set; } = null!;

    /// <summary>
    /// 租户名称
    /// </summary>
    /// <example>我的租户</example>
    [Required(ErrorMessage = "租户名称不能为空")]
    [MinLength(2, ErrorMessage = "租户名称不能少于2个字符")]
    public string TenantName { get; set; } = null!;
}
