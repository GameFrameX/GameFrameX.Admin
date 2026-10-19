using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Client.Api.GameUser.Dto;

/// <summary>
/// Facebook账号绑定输入参数
/// </summary>
public sealed class BindFacebookGameUserRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required(ErrorMessage = "用户ID不能为空")]
    public long UserId { get; set; }

    /// <summary>
    /// Facebook账号ID
    /// </summary>
    [Required(ErrorMessage = "Facebook账号ID不能为空")]
    public string FacebookId { get; set; } = null!;

    /// <summary>
    /// Facebook账号邮箱
    /// </summary>
    public string FacebookEmail { get; set; } = null!;

    /// <summary>
    /// Facebook账号名称
    /// </summary>
    public string FacebookName { get; set; } = null!;
}