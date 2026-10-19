namespace GameFrameX.ObjectStorage.Abstract;

/// <summary>
/// 单个配置字段的前端表单描述。Type 取值：text | secret | multiline | number | bool | select。
/// </summary>
public sealed class ObjectStorageConfigField
{
    /// <summary>字段键（Option 类型属性名）。</summary>
    public string Key { get; set; } = null!;

    /// <summary>展示标签。</summary>
    public string Label { get; set; } = null!;

    /// <summary>表单类型。</summary>
    public string Type { get; set; } = null!;

    /// <summary>是否必填。</summary>
    public bool Required { get; set; }

    /// <summary>帮助文本。</summary>
    public string Help { get; set; } = null!;

    /// <summary>默认值。</summary>
    public object Default { get; set; } = null!;

    /// <summary>select 类型的可选枚举。</summary>
    public string[] Options { get; set; } = null!;

    /// <summary>是否敏感字段。</summary>
    public bool Sensitive { get; set; }
}