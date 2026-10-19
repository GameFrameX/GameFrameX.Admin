using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Client.Api.GameUser.Dto;

/// <summary>
/// 谷歌账号登录输入参数
/// </summary>
public class LoginGoogleRequest
{
    /// <summary>
    /// 谷歌账号ID
    /// </summary>
    [Required(ErrorMessage = "谷歌账号ID不能为空")]
    public string GoogleId { get; set; } = null!;

    /// <summary>
    /// 谷歌账号邮箱
    /// </summary>
    public string GoogleEmail { get; set; } = null!;

    /// <summary>
    /// 谷歌账号名称
    /// </summary>
    public string GoogleName { get; set; } = null!;
}