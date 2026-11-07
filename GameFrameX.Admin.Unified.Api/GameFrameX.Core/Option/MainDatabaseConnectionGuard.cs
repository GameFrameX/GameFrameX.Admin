using GameFrameX.Core.Extension.Const;
using SqlSugar;

namespace GameFrameX.Core.Option;

/// <summary>
/// 主库连接清单的启动期守卫：ConfigId 补默认、单 main 强制、provider 与连接串风格一致性校验。
/// </summary>
/// <remarks>
/// 三个 host 的数据库装配（SqlSugarSetup 与 HostMainDbConnectionResolver）共用本守卫；
/// 环境变量与命令行参数的合并与成对校验由 LauncherOptions 统一入口负责，本守卫不再读取任何环境变量。
/// </remarks>
public static class MainDatabaseConnectionGuard
{
    /// <summary>
    /// 规整并校验连接清单：非「恰好一个 main」或 provider 与连接串风格错配时抛 <see cref="InvalidOperationException"/>。
    /// </summary>
    /// <param name="connectionConfigs">待校验的连接清单。</param>
    /// <returns>原清单（已补默认 ConfigId）。</returns>
    public static List<DbConnectionConfig> EnsureSingleMainDatabase(List<DbConnectionConfig> connectionConfigs)
    {
        foreach (var dbConfig in connectionConfigs)
        {
            if (string.IsNullOrWhiteSpace(dbConfig.ConfigId?.ToString()))
            {
                dbConfig.ConfigId = SqlSugarConst.MainConfigId;
            }
        }

        if (connectionConfigs.Count != 1 || connectionConfigs[0].ConfigId?.ToString() != SqlSugarConst.MainConfigId)
        {
            throw new InvalidOperationException("仅允许配置一个 main 主库连接。");
        }

        ValidateProviderConnectionStringConsistency(connectionConfigs);
        return connectionConfigs;
    }

    /// <summary>
    /// 校验 DbType 与连接串风格一致，把「半套覆盖」（只改了类型或只改了连接串）在启动期暴露为异常，
    /// 避免运行到首个数据库访问才报 provider 错误。
    /// 仅识别 Sqlite ↔ DataSource= 串、PostgreSQL ↔ Host= 串两种已知部署错配，新增数据库类型时按需扩展。
    /// </summary>
    private static void ValidateProviderConnectionStringConsistency(List<DbConnectionConfig> connectionConfigs)
    {
        foreach (var config in connectionConfigs)
        {
            if (string.IsNullOrWhiteSpace(config.ConnectionString))
            {
                continue;
            }

            var isSqliteStyle = config.ConnectionString.StartsWith("DataSource=", StringComparison.OrdinalIgnoreCase)
                                || config.ConnectionString.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase);

            var configId = config.ConfigId?.ToString() ?? "?";
            if (config.DbType == DbType.Sqlite && !isSqliteStyle)
            {
                throw new InvalidOperationException(
                    $"数据库连接配置 {configId} 的 DbType 为 Sqlite，但连接串不是 DataSource= 风格（\"{config.ConnectionString[..Math.Min(30, config.ConnectionString.Length)]}...\"）。" +
                    "切换数据库时 --DbType 与 --ConnectionString（环境变量形态 GFX_MAIN_DB_TYPE 与 GFX_MAIN_DB_CONNECTION_STRING）必须成对设置。");
            }

            if (config.DbType == DbType.PostgreSQL && isSqliteStyle)
            {
                throw new InvalidOperationException(
                    $"数据库连接配置 {configId} 的 DbType 为 PostgreSQL，但连接串是 DataSource= 风格（\"{config.ConnectionString[..Math.Min(30, config.ConnectionString.Length)]}...\"）。" +
                    "切换数据库时 --DbType 与 --ConnectionString（环境变量形态 GFX_MAIN_DB_TYPE 与 GFX_MAIN_DB_CONNECTION_STRING）必须成对设置。");
            }
        }
    }
}
