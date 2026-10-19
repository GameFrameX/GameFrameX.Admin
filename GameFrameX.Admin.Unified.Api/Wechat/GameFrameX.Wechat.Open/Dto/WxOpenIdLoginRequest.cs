namespace GameFrameX.Wechat.Open.Dto;

/// <summary>
/// 微信小程序登录
/// </summary>
public class WxOpenIdLoginRequest
{
    /// <summary>
    /// OpenId
    /// </summary>
    [Required(ErrorMessage = "微信标识不能为空")]
    [MinLength(10, ErrorMessage = "微信标识错误")]
    public string OpenId { get; set; } = string.Empty;
}