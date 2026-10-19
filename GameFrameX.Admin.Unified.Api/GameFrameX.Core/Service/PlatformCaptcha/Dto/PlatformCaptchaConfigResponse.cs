namespace GameFrameX.Core.Service.PlatformCaptcha.Dto;

/// <summary>
/// 平台图形验证码配置输出。
/// </summary>
public sealed class PlatformCaptchaConfigResponse
{
    /// <summary>
    /// 是否启用登录图形验证码。
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 配置版本。
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// 验证码类型。
    /// </summary>
    public int CaptchaType { get; set; }

    /// <summary>
    /// 验证码长度。
    /// </summary>
    public int CodeLength { get; set; }

    /// <summary>
    /// 过期秒数。
    /// </summary>
    public int ExpirySeconds { get; set; }

    /// <summary>
    /// 是否忽略大小写。
    /// </summary>
    public bool IgnoreCase { get; set; }

    /// <summary>
    /// 图像配置。
    /// </summary>
    public PlatformCaptchaImageRequest ImageOption { get; set; } = new();

    /// <summary>
    /// 最近更新时间。
    /// </summary>
    public long? UpdateTime { get; set; }
}