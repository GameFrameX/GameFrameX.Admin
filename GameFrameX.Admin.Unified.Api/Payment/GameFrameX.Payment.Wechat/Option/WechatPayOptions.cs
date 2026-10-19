using SKIT.FlurlHttpClient.Wechat.TenpayV3;

namespace GameFrameX.Payment.Wechat.Option;

/// <summary>
/// 微信支付配置选项
/// </summary>
public sealed class WechatPayOptions : WechatTenpayClientOptions
{
    /// <summary>
    /// 微信公众平台AppId、开放平台AppId、小程序AppId、企业微信CorpId
    /// </summary>
    public string AppId { get; set; } = null!;
}