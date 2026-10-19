using GameFrameX.Advertisement.Abstract;

namespace GameFrameX.Advertisement.TopOn;

/// <summary>
/// TopOn 广告回调配置项。
/// </summary>
[AdvertisementCallbackProvider("TopOn", "TopOn")]
public sealed class TopOnAdvertisementCallbackOption : IAdvertisementCallbackOptionSetting
{
    [AdvertisementCallbackField(Label = "回调 URL", Required = true, Help = "广告平台后台配置的回调 URL。当前只保存配置，不启用服务端回调处理。")]
    public string callbackUrl { get; set; } = null!;

    [AdvertisementCallbackField(Label = "sec_key", Sensitive = true, Required = true, Help = "TopOn 自身服务端回调用于计算 sign 的 sec_key。")]
    public string signingSecret { get; set; } = null!;

    [AdvertisementCallbackField(Label = "用户参数", Required = true, Help = "广告 SDK 透传的游戏用户 ID 参数名。")]
    public string userIdParam { get; set; } = "user_id";

    [AdvertisementCallbackField(Label = "业务透传参数", Help = "客户端透传业务上下文的参数名。")]
    public string extraDataParam { get; set; } = "extra_data";

    [AdvertisementCallbackField(Label = "交易参数", Required = true, Help = "平台交易 ID 参数名，后续回调实现需用于幂等去重。")]
    public string transactionIdParam { get; set; } = "trans_id";

    [AdvertisementCallbackField(Label = "奖励名称参数", Help = "奖励名称参数名。")]
    public string rewardNameParam { get; set; } = "reward_name";

    [AdvertisementCallbackField(Label = "奖励数量参数", Help = "奖励数量参数名。")]
    public string rewardAmountParam { get; set; } = "reward_amount";

    [AdvertisementCallbackField(Label = "广告位参数", Help = "TopOn placement_id 参数名。")]
    public string placementIdParam { get; set; } = "placement_id";

    [AdvertisementCallbackField(Label = "广告源参数", Help = "TopOn adsource_id 参数名。")]
    public string adsourceIdParam { get; set; } = "adsource_id";

    [AdvertisementCallbackField(Label = "来源网络参数", Help = "TopOn network_firm_id 参数名，用于区分 Adx、直投和交叉推广来源。")]
    public string networkFirmIdParam { get; set; } = "network_firm_id";

    [AdvertisementCallbackField(Label = "签名参数", Required = true, Help = "TopOn sign 参数名。")]
    public string signParam { get; set; } = "sign";
}