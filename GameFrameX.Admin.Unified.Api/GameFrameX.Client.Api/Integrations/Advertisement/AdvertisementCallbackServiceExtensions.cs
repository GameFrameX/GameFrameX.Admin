using GameFrameX.Advertisement.Abstract;
using GameFrameX.Advertisement.AdMob;
using GameFrameX.Advertisement.Pangle;
using GameFrameX.Advertisement.TopOn;
using GameFrameX.Advertisement.UnityAds;

namespace GameFrameX.Client.Api.Integrations.Advertisement;

/// <summary>
/// 广告回调服务扩展（照搬 <c>PaymentChannelServiceExtensions</c>）。
/// </summary>
public static class AdvertisementCallbackServiceExtensions
{
    /// <summary>
    /// 注册广告回调平台处理器 + 工厂。加平台 = 加一行 <c>AddScoped&lt;IAdvertisementCallbackHandler, XxxHandler&gt;()</c>。
    /// </summary>
    public static IServiceCollection AddAdvertisementCallbacks(this IServiceCollection services)
    {
        // 注册所有广告回调处理器（Pangle 仅验签，不进 GameAdvertisementRewardService 发奖链路）
        services.AddScoped<IAdvertisementCallbackHandler, TopOnAdvertisementCallbackHandler>();
        services.AddScoped<IAdvertisementCallbackHandler, AdMobAdvertisementCallbackHandler>();
        services.AddScoped<IAdvertisementCallbackHandler, UnityAdsAdvertisementCallbackHandler>();
        services.AddScoped<IAdvertisementCallbackHandler, PangleAdvertisementCallbackHandler>();

        // 注册工厂
        services.AddScoped<IAdvertisementCallbackFactory, AdvertisementCallbackFactory>();

        return services;
    }
}