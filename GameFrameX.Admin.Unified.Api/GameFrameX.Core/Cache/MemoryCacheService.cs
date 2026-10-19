using GameFrameX.Core.Extension.Cache;
using Microsoft.Extensions.Caching.Memory;

namespace GameFrameX.Core.Cache;

/// <summary>
/// 基于 MemoryCache 的缓存实现
/// </summary>
public class MemoryCacheService : ICache
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly ConcurrentDictionary<string, byte> _keys = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    private readonly ConcurrentDictionary<string, object> _values = new();

    public ICollection<string> Keys
    {
        get { return _keys.Keys; }
    }

    public T Get<T>(string key)
    {
        if (!_cache.TryGetValue(key as object, out var obj) || obj == null)
        {
#pragma warning disable CS8603 // 缓存未命中时返回 default(T) 是接口契约的合法语义；T 所在接口签名跨项目不可改
            return default;
#pragma warning restore CS8603
        }

        return obj is T typed ? typed : (T)obj;
    }

    public bool Set(string key, object value)
    {
        _keys[key] = 0;
        _values[key] = value;
        _cache.Set(key, value);
        return true;
    }

    public bool Set(string key, object value, TimeSpan expire)
    {
        _keys[key] = 0;
        _values[key] = value;
        _cache.Set(key, value, expire);
        return true;
    }

    public void Add(string key, object value)
    {
        Set(key, value);
    }

    public int Remove(string key)
    {
        _keys.TryRemove(key, out _);
        _values.TryRemove(key, out _);
        _cache.Remove(key);
        return 1;
    }

    public int Remove(string[] keys)
    {
        var count = 0;
        foreach (var key in keys)
        {
            count += Remove(key);
        }

        return count;
    }

    public bool ContainsKey(string key)
    {
        return _cache.TryGetValue(key as object, out _);
    }

    public T GetOrAdd<T>(string key, Func<string, T> callback, int expire = -1)
    {
#pragma warning disable CS8600, CS8603, CS8601 // MemoryCache.TryGetValue 命中后 value 由 [MaybeNullWhen(false)] 担保非空；接口 T 不可改
        if (_cache.TryGetValue(key, out T value))
        {
            return value;
        }

        value = callback(key);
        _keys[key] = 0;
        _values[key] = value;

        if (expire > 0)
        {
            _cache.Set(key, value, TimeSpan.FromSeconds(expire));
        }
        else
        {
            _cache.Set(key, value);
        }

        return value;
#pragma warning restore CS8600, CS8603, CS8601
    }

    public bool SetExpire(string key, TimeSpan expire)
    {
        if (!_values.TryGetValue(key, out var value))
        {
            return false;
        }

        _keys[key] = 0;
        _cache.Set(key, value, expire);
        return true;
    }

    public IDisposable AcquireLock(string key, int msTimeout)
    {
        return AcquireLock(key, msTimeout, msTimeout * 2, true);
    }

    public IDisposable AcquireLock(string key, int msTimeout, int msExpire, bool waitFor)
    {
        var slim = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        var acquired = slim.Wait(msTimeout);
        return new LockReleaser(acquired, slim, key, _locks);
    }

    public IEnumerable<string> Search(string pattern, int maxCount)
    {
        var prefix = pattern.TrimEnd('*');
        var results = string.IsNullOrEmpty(prefix)
                          ? _keys.Keys
                          : _keys.Keys.Where(k => k.StartsWith(prefix));
        return results.Take(maxCount);
    }

    private sealed class LockReleaser : IDisposable
    {
        private readonly bool _acquired;
        private readonly SemaphoreSlim _slim;
        private readonly string _key;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks;

        public LockReleaser(bool acquired, SemaphoreSlim slim, string key, ConcurrentDictionary<string, SemaphoreSlim> locks)
        {
            _acquired = acquired;
            _slim = slim;
            _key = key;
            _locks = locks;
        }

        public void Dispose()
        {
            if (_acquired)
            {
                _slim.Release();
            }

            _locks.TryRemove(_key, out _);
        }
    }
}