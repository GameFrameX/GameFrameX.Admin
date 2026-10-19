namespace GameFrameX.Core.Service.Auth.Dto;

/// <summary>
/// 平台超管首装密码初始化参数
/// </summary>
public class SetupSuperAdminPasswordRequest
{
    /// <summary>
    /// 登录密码
    /// </summary>
    /// <example>Abc12345</example>
    [Required(ErrorMessage = "密码不能为空")]
    [MinLength(6, ErrorMessage = "密码不能少于6个字符")]
    public string Password { get; set; } = null!;

    /// <summary>
    /// 确认密码
    /// </summary>
    /// <example>Abc12345</example>
    [Required(ErrorMessage = "确认密码不能为空")]
    [MinLength(6, ErrorMessage = "确认密码不能少于6个字符")]
    public string ConfirmPassword { get; set; } = null!;
}
