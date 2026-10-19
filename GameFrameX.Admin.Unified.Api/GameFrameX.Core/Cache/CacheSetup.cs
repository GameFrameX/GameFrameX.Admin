using GameFrameX.Core.Extension.Cache;

namespace GameFrameX.Core.Cache;

public static class CacheSetup
{
    public static ICache AddCache(this IServiceCollection services)
    {
        var cache = new MemoryCacheService();
        services.AddSingleton<ICache>(cache);
        return cache;
    }
}