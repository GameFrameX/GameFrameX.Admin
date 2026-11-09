using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Client.Api.GameUser.Dto;

/// <summary>
/// 绑定手机号输入参数（D-29：手机号为可解绑绑定项，AccessToken 鉴权）
/// </summary>
public class BindPhoneGameUserRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required(ErrorMessage = "用户ID不能为空")]
    public long UserId { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    [Required(ErrorMessage = "手机号不能为空")]
    public string Phone { get; set; } = null!;

    /// <summary>
    /// Client API 访问凭证
    /// </summary>
    [Required(ErrorMessage = "令牌不能为空")]
    public string AccessToken { get; set; } = null!;
}
