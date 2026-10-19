using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Client.Api.GameUser.Dto;

/// <summary>
/// Facebook账号登录输入参数
/// </summary>
public class LoginFacebookRequest
{
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