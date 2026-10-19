namespace GameFrameX.ObjectStorage.Abstract;

/// <summary>
/// 描述 Provider 选项类中单个字段的元数据：标签、是否必填、是否敏感（脱敏）、是否多行、帮助文本、枚举可选值、是否忽略。
/// Schema Provider 在反射时读取这些信息生成前端表单结构，并在 NormalizeConfigJson 时按这些规则校验。
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ObjectStorageFieldAttribute : Attribute
{
    /// <summary>字段展示标签。</summary>
    public string Label { get; set; }

    /// <summary>是否必填（NormalizeConfigJson 会校验空值）。</summary>
    public bool Required { get; set; }

    /// <summary>是否敏感字段（Page/Detail 脱敏为 ******，提交 ****** 时保留原值）。</summary>
    public bool Sensitive { get; set; }

    /// <summary>是否多行文本（Schema type=multiline）。</summary>
    public bool Multiline { get; set; }

    /// <summary>帮助文本。</summary>
    public string Help { get; set; } = null!;

    /// <summary>select 类型的可选枚举值（Schema type=select）。</summary>
    public string[] Options { get; set; } = null!;

    /// <summary>是否在 Schema 与 Normalize 中忽略该字段。</summary>
    public bool Ignore { get; set; }

    public ObjectStorageFieldAttribute()
    {
        Label = string.Empty;
    }
}