using Lazy.Captcha.Core;
using Lazy.Captcha.Core.Generator;
using Lazy.Captcha.Core.Generator.Image.Option;
using SkiaSharp;

namespace GameFrameX.Core.Service.PlatformCaptcha;

/// <summary>
/// 使用数据库运行时配置驱动的图形验证码服务。
/// </summary>
public sealed class PlatformCaptchaService : ICaptcha
{
    private readonly PlatformCaptchaConfigProvider _configProvider;
    private readonly PlatformCaptchaStorage _storage;
    private readonly ConcurrentDictionary<long, CaptchaService> _services = new();

    public PlatformCaptchaService(PlatformCaptchaConfigProvider configProvider, PlatformCaptchaStorage storage)
    {
        _configProvider = configProvider;
        _storage = storage;
    }

    /// <inheritdoc />
    public CaptchaData Generate(string captchaId, int? expirySeconds = null)
    {
        var settings = _configProvider.GetRequiredSettings();
        return GetService(settings).Generate(captchaId, expirySeconds);
    }

    /// <inheritdoc />
    public bool Validate(string captchaId, string code, bool removeIfSuccess = true, bool removeIfFail = true)
    {
        var settings = _configProvider.GetRequiredSettings();
        return GetService(settings).Validate(captchaId, code, removeIfSuccess, removeIfFail);
    }

    /// <inheritdoc />
    public CaptchaData GenerateWithRateLimit(string captchaId, string rateLimitKey, int? expirySeconds = null)
    {
        var settings = _configProvider.GetRequiredSettings();
        return GetService(settings).GenerateWithRateLimit(captchaId, rateLimitKey, expirySeconds);
    }

    /// <inheritdoc />
    public Lazy.Captcha.Core.RateLimit.RateLimitResult CheckRateLimit(string rateLimitKey)
    {
        var settings = _configProvider.GetRequiredSettings();
        return GetService(settings).CheckRateLimit(rateLimitKey);
    }

    private CaptchaService GetService(PlatformCaptchaSettings settings)
    {
        return _services.GetOrAdd(settings.Version, _ => new CaptchaService(ToOptions(settings), _storage));
    }

    private static CaptchaOptions ToOptions(PlatformCaptchaSettings settings)
    {
        var image = settings.ImageOption;
        return new CaptchaOptions
        {
            CaptchaType = (CaptchaType)settings.CaptchaType,
            CodeLength = settings.CodeLength,
            ExpirySeconds = settings.ExpirySeconds,
            IgnoreCase = settings.IgnoreCase,
            StorageKeyPrefix = $"{PlatformCaptchaConfigConstants.StorageKeyPrefix}v{settings.Version}:",
            ImageOption = new CaptchaImageGeneratorOption
            {
                Animation = image.Animation,
                BackgroundColor = SKColor.Parse(NormalizeColor(image.BackgroundColor)),
                ForegroundColors = PlatformCaptchaConfigProvider.SplitColors(image.ForegroundColors)
                                                                .Select(u => SKColor.Parse(NormalizeColor(u)))
                                                                .ToList(),
                FontFamily = SKTypeface.FromFamilyName(image.FontFamily),
                FontSize = image.FontSize,
                Width = image.Width,
                Height = image.Height,
                BubbleMinRadius = image.BubbleMinRadius,
                BubbleMaxRadius = image.BubbleMaxRadius,
                BubbleCount = image.BubbleCount,
                BubbleThickness = image.BubbleThickness,
                InterferenceLineCount = image.InterferenceLineCount,
                FrameDelay = image.FrameDelay,
                Quality = image.Quality,
                TextBold = image.TextBold,
            },
        };
    }

    private static string NormalizeColor(string color)
    {
        return color.StartsWith('#') ? color : $"#{color}";
    }
}