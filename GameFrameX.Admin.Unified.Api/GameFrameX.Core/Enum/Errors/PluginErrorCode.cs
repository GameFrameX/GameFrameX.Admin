namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// Admin 动态插件错误码（段 316，见 <see cref="ErrorSegmentRegistry"/>；C265 自旧 D 码体系平移）。
/// </summary>
public enum PluginErrorCode
{
    /// <summary>
    /// 已存在同名功能或同名程序及插件
    /// </summary>
    [ErrorText("已存在同名功能或同名程序及插件", "A plugin with the same name, feature, or assembly already exists")]
    PluginAlreadyExists = 31600001,
}