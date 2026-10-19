namespace GameFrameX.Advertisement.Abstract;

/// <summary>
/// 广告回调配置字段元数据标注，用于从 Option 类反射生成前端表单 schema。
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class AdvertisementCallbackFieldAttribute : Attribute
{
    /// <summary>
    /// 字段显示名称。
    /// </summary>
    public string Label { get; set; } = null!;

    /// <summary>
    /// 是否为敏感字段。
    /// </summary>
    public bool Sensitive { get; set; }

    /// <summary>
    /// 是否为多行大文本。
    /// </summary>
    public bool Multiline { get; set; }

    /// <summary>
    /// 是否必填。
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// 帮助文案。
    /// </summary>
    public string Help { get; set; } = null!;

    /// <summary>
    /// 下拉选项原始值列表。
    /// </summary>
    public string[] Options { get; set; } = null!;

    /// <summary>
    /// 是否跳过该属性。
    /// </summary>
    public bool Ignore { get; set; }
}