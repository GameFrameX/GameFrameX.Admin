namespace GameFrameX.Wechat.Open.Dto;

public class PageWechatUserRequest : BasePageRequest
{
    /// <summary>
    /// 昵称
    /// </summary>
    public string NickName { get; set; } = string.Empty;

    /// <summary>
    /// 手机号码
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
}