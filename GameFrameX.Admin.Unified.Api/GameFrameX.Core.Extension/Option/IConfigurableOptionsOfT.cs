namespace GameFrameX.Core.Extension.Option;

/// <summary>
/// 项目配置选项标记，兼容历史可配置选项类型。
/// </summary>
public interface IConfigurableOptions<TOptions> : IConfigurableOptions where TOptions : class
{
}