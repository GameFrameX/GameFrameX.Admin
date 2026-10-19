namespace GameFrameX.Core.Service.PlatformCaptcha;

/// <summary>
/// 平台验证码私有配置载荷。
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class PlatformCaptchaSettings
{
    /// <summary>
    /// 是否启用登录图形验证码。
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 配置版本。每次保存递增，使旧图形验证码立即失效。
    /// </summary>
    public long Version { get; set; } = 1;

    /// <summary>
    /// Lazy.Captcha 验证码类型。
    /// </summary>
    public int CaptchaType { get; set; } = 10;

    /// <summary>
    /// 验证码长度。
    /// </summary>
    public int CodeLength { get; set; } = 1;

    /// <summary>
    /// 验证码有效秒数。
    /// </summary>
    public int ExpirySeconds { get; set; } = 60;

    /// <summary>
    /// 是否忽略大小写。
    /// </summary>
    public bool IgnoreCase { get; set; } = true;

    /// <summary>
    /// 图像选项。
    /// </summary>
    public PlatformCaptchaImageSettings ImageOption { get; set; } = new();
}