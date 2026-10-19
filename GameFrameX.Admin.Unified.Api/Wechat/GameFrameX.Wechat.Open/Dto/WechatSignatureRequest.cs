namespace GameFrameX.Wechat.Open.Dto;

/// <summary>
/// 获取 wx.config 签名参数（C23：必须显式传入 AppId）。
/// </summary>
public class WechatSignatureRequest : WechatAppScopedRequest
{
    /// <summary>
    /// 当前页面 URL（用于 wx.config 签名）。
    /// </summary>
    public string Url { get; set; } = string.Empty;
}