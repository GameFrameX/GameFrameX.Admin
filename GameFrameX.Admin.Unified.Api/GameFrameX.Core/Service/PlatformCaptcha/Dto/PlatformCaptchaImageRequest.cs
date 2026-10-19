namespace GameFrameX.Core.Service.PlatformCaptcha.Dto;

/// <summary>
/// 平台图形验证码图像配置输入。
/// </summary>
public sealed class PlatformCaptchaImageRequest
{
    /// <summary>
    /// 是否启用 GIF 动画。
    /// </summary>
    public bool Animation { get; set; } = true;

    /// <summary>
    /// 字号。
    /// </summary>
    [Range(12, 96)]
    public float FontSize { get; set; } = 36;

    /// <summary>
    /// 宽度。
    /// </summary>
    [Range(80, 400)]
    public int Width { get; set; } = 150;

    /// <summary>
    /// 高度。
    /// </summary>
    [Range(30, 180)]
    public int Height { get; set; } = 50;

    /// <summary>
    /// 气泡最小半径。
    /// </summary>
    [Range(0, 40)]
    public int BubbleMinRadius { get; set; } = 5;

    /// <summary>
    /// 气泡最大半径。
    /// </summary>
    [Range(0, 60)]
    public int BubbleMaxRadius { get; set; } = 10;

    /// <summary>
    /// 气泡数量。
    /// </summary>
    [Range(0, 30)]
    public int BubbleCount { get; set; } = 3;

    /// <summary>
    /// 气泡线条厚度。
    /// </summary>
    [Range(0, 10)]
    public float BubbleThickness { get; set; } = 1;

    /// <summary>
    /// 干扰线数量。
    /// </summary>
    [Range(0, 30)]
    public int InterferenceLineCount { get; set; } = 3;

    /// <summary>
    /// 字体名称。
    /// </summary>
    [MaxLength(EntityStringLength.Length256)]
    public string FontFamily { get; set; } = "kaiti";

    /// <summary>
    /// 动画帧延迟。
    /// </summary>
    [Range(50, 2000)]
    public int FrameDelay { get; set; } = 300;

    /// <summary>
    /// 背景颜色。
    /// </summary>
    [MaxLength(EntityStringLength.Length256)]
    public string BackgroundColor { get; set; } = "#ffffff";

    /// <summary>
    /// 前景颜色，多个颜色用逗号分隔。
    /// </summary>
    [MaxLength(EntityStringLength.Length256)]
    public string ForegroundColors { get; set; } = string.Empty;

    /// <summary>
    /// 图片质量。
    /// </summary>
    [Range(1, 100)]
    public int Quality { get; set; } = 100;

    /// <summary>
    /// 是否粗体。
    /// </summary>
    public bool TextBold { get; set; } = true;
}