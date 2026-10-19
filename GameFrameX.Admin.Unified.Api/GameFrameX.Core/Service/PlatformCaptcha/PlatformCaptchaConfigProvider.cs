using GameFrameX.Core.Service.Cache;
using GameFrameX.Entity.System;
using Lazy.Captcha.Core.Generator;

namespace GameFrameX.Core.Service.PlatformCaptcha;

/// <summary>
/// 平台图形验证码运行时配置提供器。
/// </summary>
public sealed class PlatformCaptchaConfigProvider : IPlatformCaptchaConfigProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private const string CacheKey = PlatformCaptchaConfigConstants.ConfigCode + ":runtime";
    private readonly SqlSugarRepository<SystemConfigEntity> _systemConfigRepository;
    private readonly SystemCacheService _systemCacheService;

    public PlatformCaptchaConfigProvider(SqlSugarRepository<SystemConfigEntity> systemConfigRepository, SystemCacheService systemCacheService)
    {
        _systemConfigRepository = systemConfigRepository;
        _systemCacheService = systemCacheService;
    }

    /// <summary>
    /// 获取当前平台验证码配置。缺失或格式无效时拒绝继续使用验证码链路。
    /// </summary>
    public async Task<PlatformCaptchaSettings> GetRequiredSettingsAsync()
    {
        var cached = _systemCacheService.Get<PlatformCaptchaSettings>(CacheKey);
        if (cached != null)
        {
            return cached;
        }

        var config = await _systemConfigRepository.GetFirstAsync(u => u.Code == PlatformCaptchaConfigConstants.ConfigCode);
        return CacheRequiredSettings(config);
    }

    /// <summary>
    /// 同步获取当前平台验证码配置（缓存优先；缓存缺失时走 SqlSugar 同步读取）。
    /// 供同步 <c>ICaptcha</c> 实现使用，避免在同步调用栈上 <c>.GetAwaiter().GetResult()</c> 阻塞。
    /// </summary>
    public PlatformCaptchaSettings GetRequiredSettings()
    {
        var cached = _systemCacheService.Get<PlatformCaptchaSettings>(CacheKey);
        if (cached != null)
        {
            return cached;
        }

        var config = _systemConfigRepository.GetFirst(u => u.Code == PlatformCaptchaConfigConstants.ConfigCode);
        return CacheRequiredSettings(config);
    }

    /// <summary>
    /// 校验、缓存并返回平台验证码配置；配置缺失或格式无效时抛业务异常。
    /// </summary>
    private PlatformCaptchaSettings CacheRequiredSettings(SystemConfigEntity config)
    {
        if (config == null || string.IsNullOrWhiteSpace(config.Value))
        {
            throw BusinessError.Oh("平台图形验证码配置缺失，请先完成部署配置").StatusCode(500);
        }

        var settings = Deserialize(config.Value);
        Validate(settings);
        _systemCacheService.Set(CacheKey, settings);
        return settings;
    }

    /// <summary>
    /// 删除当前 Host 内的验证码配置缓存。
    /// </summary>
    public void Invalidate()
    {
        _systemCacheService.Remove(CacheKey);
    }

    /// <summary>
    /// 反序列化平台验证码配置。
    /// </summary>
    public static PlatformCaptchaSettings Deserialize(string value)
    {
        try
        {
            return JsonSerializer.Deserialize<PlatformCaptchaSettings>(value, SerializerOptions) ?? throw new JsonException();
        }
        catch (JsonException)
        {
            throw BusinessError.Oh("平台图形验证码配置格式无效").StatusCode(500);
        }
    }

    /// <summary>
    /// 序列化并校验长度。
    /// </summary>
    public static string Serialize(PlatformCaptchaSettings settings)
    {
        Validate(settings);
        var value = JsonSerializer.Serialize(settings, SerializerOptions);
        if (value.Length > PlatformCaptchaConfigConstants.MaxSerializedLength)
        {
            throw BusinessError.Oh($"平台图形验证码配置过长，不能超过 {PlatformCaptchaConfigConstants.MaxSerializedLength} 个字符");
        }

        return value;
    }

    /// <summary>
    /// 校验平台验证码配置。
    /// </summary>
    public static void Validate(PlatformCaptchaSettings settings)
    {
        if (settings == null)
        {
            throw BusinessError.Oh("平台图形验证码配置不能为空").StatusCode(500);
        }

        if (!global::System.Enum.IsDefined(typeof(CaptchaType), settings.CaptchaType))
        {
            throw BusinessError.Oh("平台图形验证码类型无效").StatusCode(500);
        }

        if (settings.Version <= 0)
        {
            throw BusinessError.Oh("平台图形验证码配置版本无效").StatusCode(500);
        }

        if (settings.CodeLength <= 0 || settings.CodeLength > 8)
        {
            throw BusinessError.Oh("平台图形验证码长度必须在 1 到 8 之间").StatusCode(500);
        }

        if (settings.ExpirySeconds < 10 || settings.ExpirySeconds > 600)
        {
            throw BusinessError.Oh("平台图形验证码有效期必须在 10 到 600 秒之间").StatusCode(500);
        }

        ValidateImage(settings.ImageOption);
    }

    private static void ValidateImage(PlatformCaptchaImageSettings image)
    {
        if (image == null)
        {
            throw BusinessError.Oh("平台图形验证码图像配置不能为空").StatusCode(500);
        }

        if (image.Width < 80 || image.Width > 400 || image.Height < 30 || image.Height > 180)
        {
            throw BusinessError.Oh("平台图形验证码图片尺寸无效").StatusCode(500);
        }

        if (image.FontSize < 12 || image.FontSize > 96)
        {
            throw BusinessError.Oh("平台图形验证码字号无效").StatusCode(500);
        }

        if (image.BubbleMinRadius < 0 || image.BubbleMaxRadius < 0 || image.BubbleMinRadius > image.BubbleMaxRadius)
        {
            throw BusinessError.Oh("平台图形验证码气泡半径无效").StatusCode(500);
        }

        if (image.BubbleCount < 0 || image.BubbleCount > 30 || image.InterferenceLineCount < 0 || image.InterferenceLineCount > 30)
        {
            throw BusinessError.Oh("平台图形验证码干扰参数无效").StatusCode(500);
        }

        if (image.BubbleThickness < 0 || image.BubbleThickness > 10)
        {
            throw BusinessError.Oh("平台图形验证码气泡线条厚度无效").StatusCode(500);
        }

        if (image.FrameDelay < 50 || image.FrameDelay > 2000 || image.Quality < 1 || image.Quality > 100)
        {
            throw BusinessError.Oh("平台图形验证码动画或质量参数无效").StatusCode(500);
        }

        if (string.IsNullOrWhiteSpace(image.FontFamily) || image.FontFamily.Length > 32)
        {
            throw BusinessError.Oh("平台图形验证码字体无效").StatusCode(500);
        }

        ValidateColor(image.BackgroundColor, true);
        foreach (var color in SplitColors(image.ForegroundColors))
        {
            ValidateColor(color, false);
        }
    }

    internal static string[] SplitColors(string colors)
    {
        return string.IsNullOrWhiteSpace(colors)
                   ? Array.Empty<string>()
                   : colors.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static void ValidateColor(string color, bool required)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            if (required)
            {
                throw BusinessError.Oh("平台图形验证码颜色不能为空").StatusCode(500);
            }

            return;
        }

        if (!Regex.IsMatch(color, "^#?([0-9a-fA-F]{3,4}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$"))
        {
            throw BusinessError.Oh("平台图形验证码颜色格式无效").StatusCode(500);
        }
    }
}