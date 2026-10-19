namespace GameFrameX.ObjectStorage.Abstract;

/// <summary>
/// Provider 注册项：稳定 Provider 名、展示标签、对应的 Option 选项类型。
/// Provider 名始终为声明处的标准值（小写），注册表查找大小写无关。
/// </summary>
public sealed class ObjectStorageProviderBinding
{
    /// <summary>Provider 稳定名称（标准值）。</summary>
    public string Provider { get; }

    /// <summary>展示标签。</summary>
    public string Label { get; }

    /// <summary>Option 选项类型，用于 Schema 反射与 ConfigJson 反序列化。</summary>
    public Type OptionType { get; }

    public ObjectStorageProviderBinding(string provider, string label, Type optionType)
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        Label = label ?? throw new ArgumentNullException(nameof(label));
        OptionType = optionType ?? throw new ArgumentNullException(nameof(optionType));
    }
}