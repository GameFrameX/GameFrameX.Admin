namespace GameFrameX.ObjectStorage.Abstract;

/// <summary>
/// Provider 下拉项。Value 为标准 Provider 名，Label 为展示文本。
/// </summary>
public sealed class ObjectStorageProviderItem
{
    public string Value { get; set; } = null!;

    public string Label { get; set; } = null!;
}