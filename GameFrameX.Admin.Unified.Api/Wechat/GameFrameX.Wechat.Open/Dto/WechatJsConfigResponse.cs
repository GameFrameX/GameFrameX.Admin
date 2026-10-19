namespace GameFrameX.Wechat.Open.Dto;

/// <summary>
/// 微信 JS-SDK wx.config 签名参数
/// </summary>
public class WechatJsConfigResponse
{
    /// <summary>
    /// 公众号 AppId
    /// </summary>
    public string AppId { get; set; } = null!;

    /// <summary>
    /// 生成签名用时间戳
    /// </summary>
    public string Timestamp { get; set; } = null!;

    /// <summary>
    /// 生成签名用随机字符串
    /// </summary>
    public string NonceStr { get; set; } = null!;

    /// <summary>
    /// JS-SDK 签名
    /// </summary>
    public string Signature { get; set; } = null!;
}