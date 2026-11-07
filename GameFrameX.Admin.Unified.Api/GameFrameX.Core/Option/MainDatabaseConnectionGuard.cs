using GameFrameX.Core.Extension.Const;
using SqlSugar;

namespace GameFrameX.Core.Option;

/// <summary>
/// 主库连接配置的启动期守卫：ConfigId 固定 main、连接串必填、provider 与连接串风格一致性校验。
/// </summary>
/// <remarks>
/// 三个 host 的数据库装配（SqlSugarSetup 与 HostMainDbConnectionResolver）共用本守卫；
/// 环境变量与命令行参数的合并与成对校验由 LauncherOptions 统一入口负责，本守卫不再读取任何环境变量。
/// </remarks>
public static class MainDatabaseConnectionGuard
{
    /// <summary>
    /// 规整并校验主库连接：ConfigId 缺省补 main，非 main、连接串缺失或 provider 与连接串风格错配时抛 <see cref="InvalidOperationException"/>。
    /// </summary>
    /// <param name="connectionConfig">待校验的主库连接配置。</param>
    /// <returns>原配置（已补默认 ConfigId）。</returns>
    public static DbConnectionConfig EnsureMainDatabase(DbConnectionConfig connectionConfig)
    {
        if (string.IsNullOrWhiteSpace(connectionConfig.ConfigId?.ToString()))
        {
            connectionConfig.ConfigId = SqlSugarConst.MainConfigId;
        }

        if (connectionConfig.ConfigId?.ToString() != SqlSugarConst.MainConfigId)
        {
            throw new InvalidOperationException("主库标识固定为 main，不允许配置其他连接。");
        }

        if (string.IsNullOrWhiteSpace(connectionConfig.ConnectionString))
        {
            throw new InvalidOperationException(
                "数据库主库连接配置缺失：请通过启动参数 --DbType + --ConnectionString（或 GFX_MAIN_DB_TYPE + GFX_MAIN_DB_CONNECTION_STRING 成对环境变量）提供，或在 DbConnection 配置节 / 输出目录共享 Database.json 中配置。");
        }

        ValidateProviderConnectionStringConsistency(connectionConfig);
        return connectionConfig;
    }

    /// <summary>
    /// 校验 DbType 与连接串风格一致，把「半套覆盖」（只改了类型或只改了连接串）在启动期暴露为异常，
    /// 避免运行到首个数据库访问才报 provider 错误。
    /// 仅识别 Sqlite ↔ DataSource= 串、PostgreSQL ↔ Host= 串两种已知部署错配，新增数据库类型时按需扩展。
    /// </summary>
    private static void ValidateProviderConnectionStringConsistency(DbConnectionConfig connectionConfig)
    {
        var connectionString = connectionConfig.ConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var isSqliteStyle = connectionString.StartsWith("DataSource=", StringComparison.OrdinalIgnoreCase)
                            || connectionString.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase);

        var configId = connectionConfig.ConfigId?.ToString() ?? "?";
        if (connectionConfig.DbType == DbType.Sqlite && !isSqliteStyle)
        {
            throw new InvalidOperationException(
                $"数据库连接配置 {configId} 的 DbType 为 Sqlite，但连接串不是 DataSource= 风格（\"{connectionString[..Math.Min(30, connectionString.Length)]}...\"）。" +
                "切换数据库时 --DbType 与 --ConnectionString（环境变量形态 GFX_MAIN_DB_TYPE 与 GFX_MAIN_DB_CONNECTION_STRING）必须成对设置。");
        }

        if (connectionConfig.DbType == DbType.PostgreSQL && isSqliteStyle)
        {
            throw new InvalidOperationException(
                $"数据库连接配置 {configId} 的 DbType 为 PostgreSQL，但连接串是 DataSource= 风格（\"{connectionString[..Math.Min(30, connectionString.Length)]}...\"）。" +
                "切换数据库时 --DbType 与 --ConnectionString（环境变量形态 GFX_MAIN_DB_TYPE 与 GFX_MAIN_DB_CONNECTION_STRING）必须成对设置。");
        }
    }
}
