using GameFrameX.Advertisement.Abstract;

namespace GameFrameX.Client.Api.Integrations.Advertisement;

/// <summary>
/// 广告回调处理器工厂实现：DI 注入所有 <see cref="IAdvertisementCallbackHandler"/>，按 <see cref="IAdvertisementCallbackHandler.ProviderName"/> 建字典路由。
/// </summary>
public class AdvertisementCallbackFactory : IAdvertisementCallbackFactory
{
    private readonly Dictionary<string, IAdvertisementCallbackHandler> _handlers;

    public AdvertisementCallbackFactory(IEnumerable<IAdvertisementCallbackHandler> handlers)
    {
        _handlers = handlers.ToDictionary(h => h.ProviderName, h => h, StringComparer.OrdinalIgnoreCase);
    }

    public IAdvertisementCallbackHandler? GetHandler(string providerName)
    {
        _handlers.TryGetValue(providerName, out var handler);
#pragma warning disable CS8603 // 接口签名约定 not found 返回 null
        return handler;
#pragma warning restore CS8603
    }

    public bool IsSupported(string providerName)
    {
        return _handlers.ContainsKey(providerName);
    }

    public IEnumerable<string> GetSupportedProviders()
    {
        return _handlers.Keys;
    }
}