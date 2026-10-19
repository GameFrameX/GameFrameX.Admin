namespace GameFrameX.Advertisement.Abstract;

/// <summary>
/// 广告回调 Provider 清单项。
/// </summary>
public sealed class AdvertisementCallbackProviderItem
{
    /// <summary>
    /// Provider 值。
    /// </summary>
    public string Value { get; set; } = null!;

    /// <summary>
    /// Provider 显示名。
    /// </summary>
    public string Label { get; set; } = null!;
}