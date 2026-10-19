using GameFrameX.Advertisement.Abstract;

namespace GameFrameX.Advertisement.AdMob;

/// <summary>
/// Google AdMob 广告回调配置项。
/// </summary>
[AdvertisementCallbackProvider("GoogleAdMob", "Google AdMob")]
public sealed class GoogleAdMobAdvertisementCallbackOption : IAdvertisementCallbackOptionSetting
{
    [AdvertisementCallbackField(Label = "回调 URL", Required = true, Help = "AdMob SSV 后台配置的回调 URL。当前只保存配置，不启用服务端回调处理。")]
    public string callbackUrl { get; set; } = null!;

    [AdvertisementCallbackField(Label = "验证公钥集合", Multiline = true, Sensitive = true, Required = true, Help = "AdMob key_id 到 base64 key 的映射，例如 key_id=base64key,key_id2=base64key2。")]
    public string verificationKeys { get; set; } = null!;

    [AdvertisementCallbackField(Label = "用户参数", Required = true, Help = "AdMob user_id 参数名。")]
    public string userIdParam { get; set; } = "user_id";

    [AdvertisementCallbackField(Label = "业务透传参数", Help = "AdMob custom_data 参数名。")]
    public string customDataParam { get; set; } = "custom_data";

    [AdvertisementCallbackField(Label = "交易参数", Required = true, Help = "AdMob transaction_id 参数名，后续回调实现需用于幂等去重。")]
    public string transactionIdParam { get; set; } = "transaction_id";

    [AdvertisementCallbackField(Label = "奖励类型参数", Help = "AdMob reward_item 参数名。")]
    public string rewardItemParam { get; set; } = "reward_item";

    [AdvertisementCallbackField(Label = "奖励数量参数", Help = "AdMob reward_amount 参数名。")]
    public string rewardAmountParam { get; set; } = "reward_amount";

    [AdvertisementCallbackField(Label = "广告单元参数", Help = "AdMob ad_unit 参数名。")]
    public string adUnitParam { get; set; } = "ad_unit";

    [AdvertisementCallbackField(Label = "广告网络参数", Help = "AdMob ad_network 参数名。")]
    public string adNetworkParam { get; set; } = "ad_network";

    [AdvertisementCallbackField(Label = "Key ID 参数", Required = true, Help = "AdMob key_id 参数名。")]
    public string keyIdParam { get; set; } = "key_id";

    [AdvertisementCallbackField(Label = "签名参数", Required = true, Help = "AdMob signature 参数名。")]
    public string signatureParam { get; set; } = "signature";
}