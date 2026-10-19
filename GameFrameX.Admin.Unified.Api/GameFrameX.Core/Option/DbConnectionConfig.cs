namespace GameFrameX.Core.Option;

/// <summary>
/// 数据库连接配置
/// </summary>
public sealed class DbConnectionConfig : ConnectionConfig
{
    /// <summary>
    /// 数据库设置。默认非 null：配置文件省略 Settings 节点时按代码默认值初始化，避免绑定后 NRE。
    /// </summary>
    public DbSettings DbSettings { get; set; } = new();

    /// <summary>
    /// 表设置。默认非 null：配置文件省略 Settings 节点时按代码默认值初始化，避免绑定后 NRE。
    /// </summary>
    public TableSettings TableSettings { get; set; } = new();

    /// <summary>
    /// 种子设置。默认非 null：配置文件省略 Settings 节点时按代码默认值初始化，避免绑定后 NRE。
    /// </summary>
    public SeedSettings SeedSettings { get; set; } = new();
}
