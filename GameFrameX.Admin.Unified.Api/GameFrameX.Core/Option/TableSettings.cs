namespace GameFrameX.Core.Option;

/// <summary>
/// 表设置
/// </summary>
public sealed class TableSettings
{
    /// <summary>
    /// 启用表初始化。默认 true：启动即按实体 CodeFirst 建表（幂等），配置文件可省略本节点。
    /// </summary>
    public bool EnableInitTable { get; set; } = true;

    /// <summary>
    /// 启用表增量更新-特性[IncreTable]。默认 true：只初始化带 [IncreTable] 特性的实体表（收窄建表范围）。
    /// </summary>
    public bool EnableIncreTable { get; set; } = true;
}
