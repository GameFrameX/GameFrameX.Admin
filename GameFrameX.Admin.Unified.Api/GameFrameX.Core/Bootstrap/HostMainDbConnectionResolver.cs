using GameFrameX.Core.Extension.Const;

namespace GameFrameX.Core.Bootstrap;

/// <summary>
/// Client / Hub host 的主库连接解析入口（C269 单一入口装配）：
/// 统一走「DbConnection 配置节 / 共享 Database.json 文件默认 + GFX_MAIN_DB_* 环境变量覆盖 + 错配校验」，
/// 与 Web.Api 的 <c>SqlSugarSetup.AddSqlSugar</c> 共享同一套 <see cref="DbConnectionOptions.PostConfigure"/> 管线。
/// </summary>
public static class HostMainDbConnectionResolver
{
    /// <summary>
    /// 解析主库连接配置：优先 DbConnection 节与 GFX_MAIN_DB_* 成对环境变量。
    /// </summary>
    /// <param name="configuration">host 配置（Client / Hub 通常无显式 DbConnection 节，节为空时读输出目录共享 Database.json 默认）。</param>
    /// <returns>主库连接配置（ConfigId=main）。</returns>
    public static DbConnectionConfig Resolve(IConfiguration configuration)
    {
        return Resolve(configuration, AppContext.BaseDirectory);
    }

    /// <summary>
    /// 解析主库连接配置（可注入基目录的测试入口）。优先级从低到高：
    /// 共享 Database.json 文件默认（C279 起，输出目录 <c>Configuration/Database.json</c>，仅 DbConnection 节为空时生效）
    /// &lt; GFX_MAIN_DB_* 成对环境变量（PostConfigure 覆盖） &lt; 显式 DbConnection 配置节（存在即不读文件）。
    /// </summary>
    /// <param name="configuration">host 配置。</param>
    /// <param name="baseDirectory">共享 Database.json 的查找基目录（生产为 AppContext.BaseDirectory，单测注入临时目录）。</param>
    /// <returns>主库连接配置（ConfigId=main）。</returns>
    internal static DbConnectionConfig Resolve(IConfiguration configuration, string baseDirectory)
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

        // 无任何 main 来源（节 / 共享文件 / 环境变量）时给出明确的配置缺失报错，而非落入单连接校验的歧义提示
        if (options.ConnectionConfigs.Count == 0
            && !HasAnyEnvironmentVariable("GFX_MAIN_DB_TYPE", "GFX_MAIN_DB_CONNECTION_STRING"))
        {
            throw new InvalidOperationException(
                "数据库主库连接配置缺失：GFX_MAIN_DB_TYPE 与 GFX_MAIN_DB_CONNECTION_STRING 必须成对设置（或提供 DbConnection 配置节 main 项 / 输出目录共享 Database.json）。");
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
