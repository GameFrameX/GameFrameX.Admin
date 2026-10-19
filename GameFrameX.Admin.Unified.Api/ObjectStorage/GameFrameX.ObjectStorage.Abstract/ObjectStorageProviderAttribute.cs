namespace GameFrameX.ObjectStorage.Abstract;

/// <summary>
/// 标记一个对象存储 Provider 选项类型。声明 Provider 的稳定名称与展示标签。
/// 名称大小写无关（注册表以 OrdinalIgnoreCase 查找），但对外始终输出此处声明的标准值。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ObjectStorageProviderAttribute : Attribute
{
    /// <summary>Provider 稳定名称（小写标准值，如 local / aliyun / tencent-cos / qiniu）。</summary>
    public string Name { get; }

    /// <summary>展示用标签。</summary>
    public string Label { get; }

    public ObjectStorageProviderAttribute(string name, string label)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Label = label ?? throw new ArgumentNullException(nameof(label));
    }
}