namespace GameFrameX.Core.Service.PlatformCaptcha;

/// <summary>
/// 平台验证码图像配置。
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class PlatformCaptchaImageSettings
{
    /// <summary>
    /// 是否启用 GIF 动画。
    /// </summary>
    public bool Animation { get; set; } = true;

    /// <summary>
    /// 字号。
    /// </summary>
    public float FontSize { get; set; } = 36;

    /// <summary>
    /// 宽度。
    /// </summary>
    public int Width { get; set; } = 150;

    /// <summary>
    /// 高度。
    /// </summary>
    public int Height { get; set; } = 50;

    /// <summary>
    /// 气泡最小半径。
    /// </summary>
    public int BubbleMinRadius { get; set; } = 5;

    /// <summary>
    /// 气泡最大半径。
    /// </summary>
    public int BubbleMaxRadius { get; set; } = 10;

    /// <summary>
    /// 气泡数量。
    /// </summary>
    public int BubbleCount { get; set; } = 3;

    /// <summary>
    /// 气泡线条厚度。
    /// </summary>
    public float BubbleThickness { get; set; } = 1;

    /// <summary>
    /// 干扰线数量。
    /// </summary>
    public int InterferenceLineCount { get; set; } = 3;

    /// <summary>
    /// 字体名称。
    /// </summary>
    public string FontFamily { get; set; } = "kaiti";

    /// <summary>
    /// 动画帧延迟。
    /// </summary>
    public int FrameDelay { get; set; } = 300;

    /// <summary>
    /// 背景颜色。
    /// </summary>
    public string BackgroundColor { get; set; } = "#ffffff";

    /// <summary>
    /// 前景颜色，多个颜色用逗号分隔；空值使用库默认颜色集。
    /// </summary>
    public string ForegroundColors { get; set; } = string.Empty;

    /// <summary>
    /// 图片质量。
    /// </summary>
    public int Quality { get; set; } = 100;

    /// <summary>
    /// 是否粗体。
    /// </summary>
    public bool TextBold { get; set; } = true;
}