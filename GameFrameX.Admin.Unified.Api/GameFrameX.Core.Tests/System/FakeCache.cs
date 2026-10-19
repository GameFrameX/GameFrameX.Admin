using GameFrameX.Core.Cache;
using GameFrameX.Core.Extension.Cache;

namespace GameFrameX.Core.Tests.System;

internal sealed class FakeCache : ICache
{
    public ICollection<string> Keys
    {
        get { return Array.Empty<string>(); }
    }

    public T Get<T>(string key)
    {
        return default!;
    }

    public bool Set(string key, object value)
    {
        return true;
    }

    public bool Set(string key, object value, TimeSpan expire)
    {
        return true;
    }

    public void Add(string key, object value)
    {
    }

    public int Remove(string key)
    {
        return 0;
    }

    public int Remove(string[] keys)
    {
        return 0;
    }

    public bool ContainsKey(string key)
    {
        return false;
    }

    public T GetOrAdd<T>(string key, Func<string, T> callback, int expire = -1)
    {
        return callback(key);
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