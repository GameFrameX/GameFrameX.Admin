namespace GameFrameX.Application.System.Wechat.Dto;

/// <summary>
/// JSAPI / 小程序调起支付参数
/// </summary>
public class WechatPayJsapiResponse
{
    /// <summary>
    /// 公众号/小程序 AppId
    /// </summary>
    public string AppId { get; set; } = null!;

    /// <summary>
    /// 时间戳
    /// </summary>
    public string TimeStamp { get; set; } = null!;

    /// <summary>
    /// 随机字符串
    /// </summary>
    public string NonceStr { get; set; } = null!;

    /// <summary>
    /// 订单详情扩展字符串（prepay_id=xxx）
    /// </summary>
    public string Package { get; set; } = null!;

    /// <summary>
    /// 签名方式（RSA）
    /// </summary>
    public string SignType { get; set; } = null!;

    /// <summary>
    /// 支付签名
    /// </summary>
    public string PaySign { get; set; } = null!;
}