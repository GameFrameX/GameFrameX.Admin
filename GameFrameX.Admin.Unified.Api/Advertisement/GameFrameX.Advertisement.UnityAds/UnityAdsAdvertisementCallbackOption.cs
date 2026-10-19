using GameFrameX.Advertisement.Abstract;

namespace GameFrameX.Advertisement.UnityAds;

/// <summary>
/// Unity Ads 广告回调配置项。
/// </summary>
[AdvertisementCallbackProvider("UnityAds", "Unity Ads")]
public sealed class UnityAdsAdvertisementCallbackOption : IAdvertisementCallbackOptionSetting
{
    [AdvertisementCallbackField(Label = "回调 URL", Required = true, Help = "Unity Ads S2S 后台配置的回调 URL。当前只保存配置，不启用服务端回调处理。")]
    public string callbackUrl { get; set; } = null!;

    [AdvertisementCallbackField(Label = "Secret", Sensitive = true, Required = true, Help = "Unity Ads Support/后台提供的 S2S secret，用于校验 hmac。")]
    public string signingSecret { get; set; } = null!;

    [AdvertisementCallbackField(Label = "用户/会话参数", Required = true, Help = "Unity Ads sid 参数名，通常用于映射用户或会话。")]
    public string sidParam { get; set; } = "sid";

    [AdvertisementCallbackField(Label = "订单参数", Required = true, Help = "Unity Ads oid 参数名，后续回调实现需用于幂等去重。")]
    public string orderIdParam { get; set; } = "oid";

    [AdvertisementCallbackField(Label = "HMAC 参数", Required = true, Help = "Unity Ads hmac 参数名。")]
    public string hmacParam { get; set; } = "hmac";
}