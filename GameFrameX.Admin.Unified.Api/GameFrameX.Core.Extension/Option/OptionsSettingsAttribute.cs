namespace GameFrameX.Core.Extension.Option;

/// <summary>
/// 指定配置节名称。
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class OptionsSettingsAttribute : Attribute
{
    public OptionsSettingsAttribute(string path)
    {
        Path = path;
    }

    /// <summary>
    /// 配置节路径。
    /// </summary>
    public string Path { get; }
}