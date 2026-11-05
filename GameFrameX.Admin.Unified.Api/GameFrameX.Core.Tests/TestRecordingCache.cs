using GameFrameX.Core.Extension.Cache;

namespace GameFrameX.Core.Tests;

/// <summary>
/// 可读回的测试缓存（FakeCache 不存值，无法验证 SystemCacheService 键值通道）。
/// </summary>
internal sealed class TestRecordingCache : ICache
{
    private readonly Dictionary<string, object> _values = new();

    public ICollection<string> Keys
    {
        get { return _values.Keys.ToArray(); }
    }

    public T Get<T>(string key)
    {
        return _values.TryGetValue(key, out var value) ? (T)value : default!;
    }

    public bool Set(string key, object value)
    {
        _values[key] = value;
        return true;
    }

    public bool Set(string key, object value, TimeSpan expire)
    {
        return Set(key, value);
    }

    public void Add(string key, object value)
    {
        _values[key] = value;
    }

    public int Remove(string key)
    {
        return _values.Remove(key) ? 1 : 0;
    }

    public int Remove(string[] keys)
    {
        return keys.Count(key => Remove(key) > 0);
    }

    public bool ContainsKey(string key)
    {
        return _values.ContainsKey(key);
    }

    public T GetOrAdd<T>(string key, Func<string, T> callback, int expire = -1)
    {
        if (_values.TryGetValue(key, out var value))
        {
            return (T)value;
        }

        var added = callback(key);
        _values[key] = added!;
        return added;
    }

    public bool SetExpire(string key, TimeSpan expire)
    {
        return true;
    }

    public IDisposable AcquireLock(string key, int msTimeout)
    {
        return null!;
    }

    public IDisposable AcquireLock(string key, int msTimeout, int msExpire, bool waitFor)
    {
        return null!;
    }

    public IEnumerable<string> Search(string pattern, int maxCount)
    {
        return Array.Empty<string>();
    }
}
