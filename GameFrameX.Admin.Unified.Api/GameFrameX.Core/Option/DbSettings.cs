namespace GameFrameX.Core.Option;

/// <summary>
/// 数据库设置
/// </summary>
public sealed class DbSettings
{
    /// <summary>
    /// 启用库表初始化。默认 true：配置文件省略本节点时启动即自动建库（SQLite 例外走 EnsureSqliteDataDirectory），线上与本地行为一致。
    /// </summary>
    public bool EnableInitDb { get; set; } = true;

    /// <summary>
    /// 启用库表差异日志。默认 true：写操作的差异快照经 AOP 落 system_log_diff / tenant_log_diff。
    /// </summary>
    public bool EnableDiffLog { get; set; } = true;
}
