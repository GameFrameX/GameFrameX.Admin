using GameFrameX.Core.Extension.Const;

namespace GameFrameX.Core.Bootstrap;

/// <summary>
/// Client / Hub host 的主库连接解析入口（C269 单一入口装配）：
/// 统一走「DbConnection 配置节 / 共享 Database.json 文件默认 + GFX_MAIN_DB_* 环境变量覆盖 + 默认租户库推导 + 错配校验」，
/// 与 Web.Api 的 <c>SqlSugarSetup.AddSqlSugar</c> 共享同一套 <see cref="DbConnectionOptions.PostConfigure"/> 管线；
/// <see cref="LauncherOptions"/> 裸启动参数降级为兼容兜底（无任何配置来源时才生效，并打迁移警告）。
/// </summary>
public static class HostMainDbConnectionResolver
{
    /// <summary>
    /// 解析主库连接配置：优先 DbConnection 节与 GFX_MAIN_DB_* 成对环境变量，兜底 LauncherOptions 裸参数。
    /// </summary>
    /// <param name="configuration">host 配置（Client / Hub 通常无显式 DbConnection 节，节为空时读输出目录共享 Database.json 默认）。</param>
    /// <param name="launcherOptions">进程启动选项，仅在节、共享文件默认与环境变量均无 main 来源时作兼容兜底。</param>
    /// <returns>主库连接配置（ConfigId=main）。</returns>
    public static DbConnectionConfig Resolve(IConfiguration configuration, LauncherOptions launcherOptions)
    {
        return Resolve(configuration, launcherOptions, AppContext.BaseDirectory);
    }

    /// <summary>
    /// 解析主库连接配置（可注入基目录的测试入口）。优先级从低到高：
    /// 共享 Database.json 文件默认（C279 起，输出目录 <c>Configuration/Database.json</c>，仅 DbConnection 节为空时生效）
    /// &lt; GFX_MAIN_DB_* 成对环境变量（PostConfigure 覆盖） &lt; 显式 DbConnection 配置节（存在即不读文件）。
    /// 文件默认只是补默认值层，环境变量覆盖与 LauncherOptions 兜底管线保持不变。
    /// </summary>
    /// <param name="configuration">host 配置。</param>
    /// <param name="launcherOptions">进程启动选项，仅在节、共享文件默认与环境变量均无 main 来源时作兼容兜底。</param>
    /// <param name="baseDirectory">共享 Database.json 的查找基目录（生产为 AppContext.BaseDirectory，单测注入临时目录）。</param>
    /// <returns>主库连接配置（ConfigId=main）。</returns>
    internal static DbConnectionConfig Resolve(IConfiguration configuration, LauncherOptions launcherOptions, string baseDirectory)
    {
        var options = configuration.GetSection("DbConnection").Get<DbConnectionOptions>() ?? new DbConnectionOptions();

        // C279：节为空时读输出目录共享 Database.json 作三 host 统一默认（Client/Hub 经 csproj 文件链接复制，无程序集依赖）；
        // 刻意窄加载——只读这一个文件，不复制 Web.Api 的全目录扫描，防 Application 将来的 Admin 专属配置被吸入 Client/Hub
        if (!configuration.GetSection("DbConnection").Exists())
        {
            var sharedFile = Path.Combine(baseDirectory, "Configuration", "Database.json");
            if (File.Exists(sharedFile))
            {
                options = new ConfigurationBuilder().AddJsonFile(sharedFile, false, false).Build()
                                                    .GetSection("DbConnection").Get<DbConnectionOptions>() ?? options;
            }
        }

        // 兼容层：无节、无 GFX_MAIN_DB_* 时用 LauncherOptions 裸参数兜底，给存量部署留迁移窗口
        var hasMainSource = options.ConnectionConfigs.Count > 0
                            || HasAnyEnvironmentVariable("GFX_MAIN_DB_TYPE", "GFX_MAIN_DB_CONNECTION_STRING");
        if (!hasMainSource)
        {
            if (string.IsNullOrWhiteSpace(launcherOptions.DbType) || string.IsNullOrWhiteSpace(launcherOptions.ConnectionString))
            {
                throw new InvalidOperationException(
                    "数据库主库连接配置缺失：请成对设置 GFX_MAIN_DB_TYPE + GFX_MAIN_DB_CONNECTION_STRING（或 DbConnection 配置节 main 项；存量裸启动参数 DbType/ConnectionString 亦可作兼容兜底）。");
            }

            // ponytail: 用 Console 而非 Serilog.Log.Warning——全仓 Serilog 走 DI 管道，static Log 未接线（SilentLogger 会吞警告）；
            // 升级路径：若后续启动期日志统一改 static Serilog，此处随之切换
            Console.WriteLine("[Warning] LauncherOptions 的 DbType / ConnectionString 启动参数已降级为兼容层，请迁移到 GFX_MAIN_DB_TYPE + GFX_MAIN_DB_CONNECTION_STRING 成对环境变量（或 DbConnection 配置节）。");
            options.ConnectionConfigs.Add(new DbConnectionConfig
            {
                ConfigId = SqlSugarConst.MainConfigId,
                DbType = System.Enum.Parse<DbType>(launcherOptions.DbType),
                ConnectionString = launcherOptions.ConnectionString,
            });
        }

        options.PostConfigure(options, configuration);

        var mainConfig = options.ConnectionConfigs.FirstOrDefault(item => item.ConfigId?.ToString() == SqlSugarConst.MainConfigId);
        if (mainConfig == null)
        {
            throw new InvalidOperationException(
                "数据库主库连接配置缺失：GFX_MAIN_DB_TYPE 与 GFX_MAIN_DB_CONNECTION_STRING 必须成对设置（或提供 DbConnection 配置节 main 项）。");
        }

        return mainConfig;
    }

    private static bool HasAnyEnvironmentVariable(params string[] names)
    {
        return names.Any(name => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name)));
    }
}