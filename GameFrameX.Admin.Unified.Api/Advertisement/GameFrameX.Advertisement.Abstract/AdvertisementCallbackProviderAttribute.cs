namespace GameFrameX.Advertisement.Abstract;

/// <summary>
/// 标注广告回调 Provider 的 Option 配置类，声明稳定 Provider 标识和展示名称。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AdvertisementCallbackProviderAttribute : Attribute
{
    /// <summary>
    /// Provider 唯一标识。
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Provider 展示名称。
    /// </summary>
    public string Label { get; }

    public AdvertisementCallbackProviderAttribute(string name, string label)
    {
        Name = name;
        Label = label;
    }
}