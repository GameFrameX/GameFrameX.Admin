using GameFrameX.Core.Extension.Cache;

namespace GameFrameX.Core.Cache;

/// <summary>
/// SqlSugar二级缓存
/// </summary>
public class SqlSugarCache : ICacheService
{
    private static ICache? Cache;

    public static void Configure(ICache cache)
    {
        Cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public void Add<V>(string key, V value)
    {
#pragma warning disable CS8604 // V 为 SqlSugar ICacheService 外部接口的泛型参数，可能为引用类型 null；委托至底层 ICache.Set(object) 由其处理
        GetCache().Set($"{CacheConst.SqlSugar}{key}", value);
#pragma warning restore CS8604
    }

    public void Add<V>(string key, V value, int cacheDurationInSeconds)
    {
#pragma warning disable CS8604 // V 为 SqlSugar ICacheService 外部接口的泛型参数，可能为引用类型 null；委托至底层 ICache.Set(object) 由其处理
        GetCache().Set($"{CacheConst.SqlSugar}{key}", value, TimeSpan.FromSeconds(cacheDurationInSeconds));
#pragma warning restore CS8604
    }

    public bool ContainsKey<V>(string key)
    {
        return GetCache().ContainsKey($"{CacheConst.SqlSugar}{key}");
    }

    public V Get<V>(string key)
    {
        return GetCache().Get<V>($"{CacheConst.SqlSugar}{key}");
    }

    public IEnumerable<string> GetAllKey<V>()
    {
        return GetCache().Keys.Where(key => key.StartsWith(CacheConst.SqlSugar, StringComparison.Ordinal));
    }

    public V GetOrCreate<V>(string key, Func<V> create, int cacheDurationInSeconds = int.MaxValue)
    {
        return GetCache().GetOrAdd<V>($"{CacheConst.SqlSugar}{key}", (cacheKey) => { return create(); }, cacheDurationInSeconds);
    }

    public void Remove<V>(string key)
    {
        GetCache().Remove(key); // SqlSugar调用Remove方法时，key中已包含了CacheConst.SqlSugar前缀
    }

    private static ICache GetCache()
    {
        return Cache ?? throw new InvalidOperationException("SqlSugarCache 尚未配置运行期缓存。");
    }
}