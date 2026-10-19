namespace GameFrameX.Core.Service.Auth.Dto;

/// <summary>
/// 发送邮箱自助注册验证码参数
/// </summary>
public class SendRegisterEmailCodeRequest
{
    /// <summary>
    /// 注册邮箱（同时作为租管账号）
    /// </summary>
    /// <example>admin@example.com</example>
    [Required(ErrorMessage = "邮箱不能为空")]
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string Email { get; set; } = null!;
}
