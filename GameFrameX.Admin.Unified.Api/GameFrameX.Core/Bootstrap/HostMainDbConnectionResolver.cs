using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Option;

namespace GameFrameX.Core.Bootstrap;

/// <summary>
/// Client / Hub host 的主库连接解析入口：只做配置加载与兜底，不读环境变量。
/// </summary>
/// <remarks>
/// 优先级从低到高：共享 Database.json 文件默认（仅 DbConnection 节为空时生效）
/// &lt; 显式 DbConnection 配置节（appsettings / 环境变量 / 命令行）
/// &lt; LauncherOptions 统一启动参数层（--DbType/--ConnectionString 与 GFX_MAIN_DB_*，命令行优先，经
/// <see cref="LauncherOptionsMainDatabase"/> 以最高优先级追加进配置管线）。
/// 环境变量与命令行的合并、成对校验全部由 LauncherOptions 承担；本类与 Web.Api 的 SqlSugarSetup 共用
/// <see cref="MainDatabaseConnectionGuard"/> 做单 main 与 provider 一致性校验。
/// </summary>
public static class HostMainDbConnectionResolver
{
    /// <summary>
    /// 解析主库连接配置（ConfigId=main）。
    /// </summary>
    /// <param name="configuration">host 配置。</param>
    /// <returns>主库连接配置。</returns>
    public static DbConnectionConfig Resolve(IConfiguration configuration)
    {
        return Resolve(configuration, AppContext.BaseDirectory);
    }

    /// <summary>
    /// 解析主库连接配置（可注入基目录的测试入口）。
    /// </summary>
    /// <param name="configuration">host 配置。</param>
    /// <param name="baseDirectory">共享 Database.json 的查找基目录（生产为 AppContext.BaseDirectory，单测注入临时目录）。</param>
    /// <returns>主库连接配置（ConfigId=main）。</returns>
    internal static DbConnectionConfig Resolve(IConfiguration configuration, string baseDirectory)
    {
        var connectionConfigs = LoadConnectionConfigs(configuration);

        // C279：节为空时读输出目录共享 Database.json 作三 host 统一默认（Client/Hub 经 csproj 文件链接复制，无程序集依赖）；
        // 刻意窄加载——只读这一个文件，不复制 Web.Api 的全目录扫描，防 Application 将来的 Admin 专属配置被吸入 Client/Hub
        if (connectionConfigs.Count == 0 && !configuration.GetSection("DbConnection").Exists())
        {
            var sharedFile = Path.Combine(baseDirectory, "Configuration", "Database.json");
            if (File.Exists(sharedFile))
            {
                connectionConfigs = LoadConnectionConfigs(new ConfigurationBuilder().AddJsonFile(sharedFile, false, false).Build());
            }
        }

        if (connectionConfigs.Count == 0)
        {
            throw new InvalidOperationException(
                "数据库主库连接配置缺失：请通过启动参数 --DbType + --ConnectionString（或 GFX_MAIN_DB_TYPE + GFX_MAIN_DB_CONNECTION_STRING 成对环境变量）提供，或在 DbConnection 配置节 / 输出目录共享 Database.json 中配置 main 项。");
        }

        MainDatabaseConnectionGuard.EnsureSingleMainDatabase(connectionConfigs);
        return connectionConfigs[0];
    }

    private static List<DbConnectionConfig> LoadConnectionConfigs(IConfiguration configuration)
    {
        return configuration.GetSection("DbConnection:ConnectionConfigs").Get<List<DbConnectionConfig>>() ?? new List<DbConnectionConfig>();
    }
}
