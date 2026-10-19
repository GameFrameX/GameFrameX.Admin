namespace GameFrameX.Advertisement.Abstract;

/// <summary>
/// 广告回调字段下拉选项。
/// </summary>
public sealed class AdvertisementCallbackConfigOption
{
    /// <summary>
    /// 选项值。
    /// </summary>
    public string Value { get; set; } = null!;

    /// <summary>
    /// 选项显示名。
    /// </summary>
    public string Label { get; set; } = null!;
}