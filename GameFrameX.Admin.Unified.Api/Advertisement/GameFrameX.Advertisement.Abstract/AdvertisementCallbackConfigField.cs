namespace GameFrameX.Advertisement.Abstract;

/// <summary>
/// 广告回调配置字段描述。
/// </summary>
public sealed class AdvertisementCallbackConfigField
{
    /// <summary>
    /// 字段键。
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// 显示名称。
    /// </summary>
    public string Label { get; set; } = null!;

    /// <summary>
    /// 字段类型：text | secret | multiline | number | bool | select。
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// 是否必填。
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// 帮助文案。
    /// </summary>
    public string Help { get; set; } = null!;

    /// <summary>
    /// 默认值。
    /// </summary>
    public object? Default { get; set; }

    /// <summary>
    /// 是否敏感。敏感字段输出脱敏，编辑回传 <c>******</c> 时保留原值。
    /// </summary>
    public bool Sensitive { get; set; }

    /// <summary>
    /// 下拉选项。
    /// </summary>
    public List<AdvertisementCallbackConfigOption>? Options { get; set; }
}