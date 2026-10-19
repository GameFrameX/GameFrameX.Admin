namespace GameFrameX.Core.Service.PlatformCaptcha.Dto;

/// <summary>
/// 平台图形验证码配置更新输入。
/// </summary>
public sealed class PlatformCaptchaConfigUpdateRequest
{
    /// <summary>
    /// 是否启用登录图形验证码。
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 验证码类型。
    /// </summary>
    [Range(0, 11)]
    public int CaptchaType { get; set; } = 10;

    /// <summary>
    /// 验证码长度。
    /// </summary>
    [Range(1, 8)]
    public int CodeLength { get; set; } = 1;

    /// <summary>
    /// 过期秒数。
    /// </summary>
    [Range(10, 600)]
    public int ExpirySeconds { get; set; } = 60;

    /// <summary>
    /// 是否忽略大小写。
    /// </summary>
    public bool IgnoreCase { get; set; } = true;

    /// <summary>
    /// 图像配置。
    /// </summary>
    [Required]
    public PlatformCaptchaImageRequest ImageOption { get; set; } = new();
}