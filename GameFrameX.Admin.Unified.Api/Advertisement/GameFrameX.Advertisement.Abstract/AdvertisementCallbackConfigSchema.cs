namespace GameFrameX.Advertisement.Abstract;

/// <summary>
/// 广告回调 Provider schema。
/// </summary>
public sealed class AdvertisementCallbackConfigSchema
{
    /// <summary>
    /// Provider 标识。
    /// </summary>
    public string Provider { get; set; } = null!;

    /// <summary>
    /// Provider 显示名。
    /// </summary>
    public string Label { get; set; } = null!;

    /// <summary>
    /// 动态字段列表。
    /// </summary>
    public List<AdvertisementCallbackConfigField> Fields { get; set; } = new();
}