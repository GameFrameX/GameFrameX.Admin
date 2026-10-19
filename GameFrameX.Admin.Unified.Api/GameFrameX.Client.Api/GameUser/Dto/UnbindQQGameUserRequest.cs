using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Client.Api.GameUser.Dto;

/// <summary>
/// QQ账号解绑输入参数
/// </summary>
public class UnbindQQGameUserRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required(ErrorMessage = "用户ID不能为空")]
    public long UserId { get; set; }

    /// <summary>
    /// 确认密码（可选，用于安全验证）
    /// </summary>
    public string Password { get; set; } = null!;
}