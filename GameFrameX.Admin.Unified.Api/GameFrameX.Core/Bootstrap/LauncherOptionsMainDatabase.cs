using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.SqlSugar;

namespace GameFrameX.Core.Bootstrap;

/// <summary>
/// 把 <see cref="LauncherOptions"/> 中成对提供的数据库连接参数以最高优先级写入配置管线。
/// </summary>
/// <remarks>
/// LauncherOptions 是启动参数的统一入口（Foundation OptionsBuilder 已合并命令行参数与环境变量，命令行优先）。
/// 本类只做一件事：成对校验后翻译成 DbConnection 配置节键，以最高优先级追加进配置管线；
/// 单 main 与 provider 一致性校验由 MainDatabaseConnectionGuard 在装配点统一执行，Database.json 仅作最后兜底。
/// </remarks>
public static class LauncherOptionsMainDatabase
{
    /// <summary>
    /// 启动参数未提供数据库连接时不追加任何键；成对提供时追加为最高优先级配置层；半套拒绝。
    /// </summary>
    /// <param name="builder">host 配置构建器。</param>
    /// <param name="launcherOptions">进程启动选项（已由 OptionsBuilder 合并命令行与环境变量）。</param>
    /// <returns>原配置构建器。</returns>
    public static IConfigurationBuilder AddMainDatabaseFromLauncherOptions(this IConfigurationBuilder builder, LauncherOptions launcherOptions)
    {
        var databaseType = launcherOptions.DbType;
        var connectionString = launcherOptions.ConnectionString;
        var hasDatabaseType = !string.IsNullOrWhiteSpace(databaseType);
        var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
        if (!hasDatabaseType && !hasConnectionString)
        {
            return builder;
        }

        if (!hasDatabaseType || !hasConnectionString)
        {
            throw new InvalidOperationException(
                "启动参数必须成对提供：--DbType 与 --ConnectionString（环境变量形态 GFX_MAIN_DB_TYPE + GFX_MAIN_DB_CONNECTION_STRING 同理），半套会导致 provider 与连接串错配。");
        }

        return builder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DbConnection:ConnectionConfigs:0:ConfigId"] = SqlSugarConst.MainConfigId,
            ["DbConnection:ConnectionConfigs:0:DbType"] = databaseType,
            ["DbConnection:ConnectionConfigs:0:ConnectionString"] = connectionString,
        });
    }
}
