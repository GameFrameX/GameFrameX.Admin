using Microsoft.AspNetCore.Http;

namespace GameFrameX.Advertisement.Abstract;

/// <summary>
/// 广告回调平台处理器契约（provider 模式，照搬 <c>IPaymentChannelHandler</c>）。
/// <para>各广告平台（TopOn / AdMob / UnityAds / ...）实现此接口，通过 <see cref="IAdvertisementCallbackFactory"/> 按 <see cref="ProviderName"/> 路由。</para>
/// <para>加平台 = 加实现类 + 一行 DI 注册，<c>GameAdvertisementRewardService</c> 通用流程不动。</para>
/// </summary>
public interface IAdvertisementCallbackHandler
{
    /// <summary>平台标识（路由 key，与 <c>AdvertisementCallbackProviderRegistry</c> 的 Provider 一致）。</summary>
    string ProviderName { get; }

    /// <summary>schema 取参 + 按平台算法验签。</summary>
    OptionVerification Verify(TenantAdvertisementCallbackOptions advertisementOptions, IQueryCollection query);

    /// <summary>从已验签 Option + query 解析角色 + 用户。</summary>
    Task<RoleResolution?> ResolveRoleAsync(object option, IQueryCollection query);
}