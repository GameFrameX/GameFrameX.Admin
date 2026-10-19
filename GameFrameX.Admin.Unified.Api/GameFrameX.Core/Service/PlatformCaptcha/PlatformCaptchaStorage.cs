using GameFrameX.Core.Service.Cache;
using Lazy.Captcha.Core.Storage;

namespace GameFrameX.Core.Service.PlatformCaptcha;

/// <summary>
/// 基于系统内存缓存的 Lazy.Captcha 存储适配器。
/// </summary>
public sealed class PlatformCaptchaStorage : IStorage
{
    private readonly SystemCacheService _systemCacheService;

    public PlatformCaptchaStorage(SystemCacheService systemCacheService)
    {
        _systemCacheService = systemCacheService;
    }

    /// <inheritdoc />
    public void Set(string key, string value, DateTimeOffset absoluteExpiration)
    {
        var expire = absoluteExpiration - DateTimeOffset.Now;
        if (expire <= TimeSpan.Zero)
        {
            return;
        }

        _systemCacheService.Set(key, value, expire);
    }

    /// <inheritdoc />
    public string Get(string key)
    {
        return _systemCacheService.Get<string>(key);
    }

    /// <inheritdoc />
    public void Remove(string key)
    {
        _systemCacheService.Remove(key);
    }
}