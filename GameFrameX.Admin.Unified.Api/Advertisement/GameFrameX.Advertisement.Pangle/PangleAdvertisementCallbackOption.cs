using GameFrameX.Advertisement.Abstract;

namespace GameFrameX.Advertisement.Pangle;

/// <summary>
/// Pangle 广告回调配置项。
/// </summary>
[AdvertisementCallbackProvider("Pangle", "Pangle")]
public sealed class PangleAdvertisementCallbackOption : IAdvertisementCallbackOptionSetting
{
    [AdvertisementCallbackField(Label = "回调 URL", Required = true, Help = "Pangle 后台配置的服务端回调 URL。")]
    public string callbackUrl { get; set; } = null!;

    [AdvertisementCallbackField(Label = "Security Key", Sensitive = true, Required = true, Help = "Pangle 回调用于计算 Sign 的 security key。")]
    public string signingSecret { get; set; } = null!;

    [AdvertisementCallbackField(Label = "交易参数", Required = true, Help = "Pangle 交易 ID 参数名，当前回调处理读取 TransId。")]
    public string transactionIdParam { get; set; } = "TransId";

    [AdvertisementCallbackField(Label = "签名参数", Required = true, Help = "Pangle 签名参数名，当前回调处理读取 Sign。")]
    public string signParam { get; set; } = "Sign";

    [AdvertisementCallbackField(Label = "奖励数量参数", Help = "Pangle 奖励数量参数名。")]
    public string rewardAmountParam { get; set; } = "RewardAmount";

    [AdvertisementCallbackField(Label = "奖励名称参数", Help = "Pangle 奖励名称参数名。")]
    public string rewardNameParam { get; set; } = "RewardName";

    [AdvertisementCallbackField(Label = "业务透传参数", Help = "Pangle 透传业务上下文参数名。")]
    public string extraParam { get; set; } = "Extra";
}