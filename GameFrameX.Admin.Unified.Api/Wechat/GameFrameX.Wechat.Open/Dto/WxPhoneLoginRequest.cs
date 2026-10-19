namespace GameFrameX.Wechat.Open.Dto;

/// <summary>
/// 微信手机号登录
/// </summary>
public class WxPhoneLoginRequest
{
    /// <summary>
    /// 电话号码
    /// </summary>
    [RegularExpression(@"^1[3-9]\d{9}$", ErrorMessage = "电话号码错误")]
    public string PhoneNumber { get; set; } = string.Empty;
}