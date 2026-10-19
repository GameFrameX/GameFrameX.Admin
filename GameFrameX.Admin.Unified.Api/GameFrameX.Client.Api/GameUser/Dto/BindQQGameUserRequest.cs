using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Client.Api.GameUser.Dto;

/// <summary>
/// QQ账号绑定输入参数
/// </summary>
public sealed class BindQQGameUserRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required(ErrorMessage = "用户ID不能为空")]
    public long UserId { get; set; }

    /// <summary>
    /// QQ账号ID
    /// </summary>
    [Required(ErrorMessage = "QQ账号ID不能为空")]
    public string QQId { get; set; } = null!;

    /// <summary>
    /// QQ账号邮箱
    /// </summary>
    public string QQEmail { get; set; } = null!;

    /// <summary>
    /// QQ账号名称
    /// </summary>
    public string QQName { get; set; } = null!;
}