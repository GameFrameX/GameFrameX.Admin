namespace GameFrameX.Core.Extension.Cache;

/// <summary>
/// 缓存接口（兼容 NewLife.ICache 签名）
/// </summary>
public interface ICache
{
    ICollection<string> Keys { get; }
    T Get<T>(string key);
    bool Set(string key, object value);
    bool Set(string key, object value, TimeSpan expire);
    void Add(string key, object value);
    int Remove(string key);
    int Remove(string[] keys);
    bool ContainsKey(string key);
    T GetOrAdd<T>(string key, Func<string, T> callback, int expire = -1);
    bool SetExpire(string key, TimeSpan expire);
    IDisposable AcquireLock(string key, int msTimeout);
    IDisposable AcquireLock(string key, int msTimeout, int msExpire, bool waitFor);
    IEnumerable<string> Search(string pattern, int maxCount);
}