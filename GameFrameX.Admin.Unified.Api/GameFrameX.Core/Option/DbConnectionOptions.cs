using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Extension.Option;

namespace GameFrameX.Core.Option;

/// <summary>
/// 数据库配置选项
/// </summary>
[OptionsSettings("DbConnection")]
public sealed class DbConnectionOptions
{
    /// <summary>
    /// 启用控制台打印SQL
    /// </summary>
    public bool EnableConsoleSql { get; set; }

    /// <summary>
    /// 数据库集合
    /// </summary>
    public List<DbConnectionConfig> ConnectionConfigs { get; set; } = new();

    public void PostConfigure(DbConnectionOptions options, IConfiguration configuration)
    {
        // 成对裸启动参数（--DbType/--ConnectionString）已作为最高优先级配置层写入标记：
        // 此时跳过 GFX_MAIN_DB_* 成对校验与覆盖（命令行语义压过环境变量），只保留单连接与 provider 一致性校验
        var startupArgumentsTakePrecedence = string.Equals(
            configuration["DbConnection:MainDatabaseFromStartupArguments"],
            bool.TrueString,
            StringComparison.OrdinalIgnoreCase);

        if (!startupArgumentsTakePrecedence)
        {
            if (HasEnvironmentVariable("GFX_MAIN_DB_TYPE") != HasEnvironmentVariable("GFX_MAIN_DB_CONNECTION_STRING"))
            {
                throw new InvalidOperationException("GFX_MAIN_DB_TYPE 与 GFX_MAIN_DB_CONNECTION_STRING 必须成对设置。");
            }

            ApplyEnvironmentOverrides(options);
        }

        foreach (var dbConfig in options.ConnectionConfigs)
        {
            if (string.IsNullOrWhiteSpace(dbConfig.ConfigId?.ToString()))
            {
                dbConfig.ConfigId = SqlSugarConst.MainConfigId;
            }
        }

        if (options.ConnectionConfigs.Count != 1 || options.ConnectionConfigs[0].ConfigId?.ToString() != SqlSugarConst.MainConfigId)
        {
            throw new InvalidOperationException("仅允许配置一个 main 主库连接。");
        }

        ValidateProviderConnectionStringConsistency(options);
    }

    private static void ApplyEnvironmentOverrides(DbConnectionOptions options)
    {
        // 无 Database.json 的 host（Client / Hub）：列表为空且 GFX_MAIN_DB_* 成对存在时补建 main 项，
        // 使四个 host 共享同一套覆盖、推导与校验管线；半套覆盖不建项，交由装配点按「必须成对」报错
        if (options.ConnectionConfigs.Count == 0
            && HasEnvironmentVariablePair("GFX_MAIN_DB_TYPE", "GFX_MAIN_DB_CONNECTION_STRING"))
        {
            options.ConnectionConfigs.Add(new DbConnectionConfig { ConfigId = SqlSugarConst.MainConfigId, });
        }

        if (options.ConnectionConfigs.Count == 0)
        {
            return;
        }

        // 数据库类型必须与连接串成对覆盖（如 GFX_MAIN_DB_TYPE=GFX_MAIN_DB_CONNECTION_STRING），防止 provider 错配
        ApplyDatabaseType(options, SqlSugarConst.MainConfigId, 0,
                          "GFX_MAIN_DB_TYPE");

        ApplyConnectionString(options, SqlSugarConst.MainConfigId, 0,
                              "GFX_MAIN_DB_CONNECTION_STRING",
                              "MAIN_DB_CONNECTION_STRING",
                              "ConnectionString");

        for (var i = 0; i < options.ConnectionConfigs.Count; i++)
        {
            var connectionString = Environment.GetEnvironmentVariable($"DbConnection__ConnectionConfigs__{i}__ConnectionString");
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.ConnectionConfigs[i].ConnectionString = connectionString;
            }
        }
    }

    private static void ApplyConnectionString(DbConnectionOptions options, string configId, int fallbackIndex, params string[] environmentVariableNames)
    {
        var connectionString = environmentVariableNames
                               .Select(Environment.GetEnvironmentVariable)
                               .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var config = FindConfig(options, configId, fallbackIndex);
        if (config != null)
        {
            config.ConnectionString = connectionString;
        }
    }

    /// <summary>
    /// 按库标识覆盖数据库类型，与 <see cref="ApplyConnectionString"/> 成对使用，保证 provider 与连接串一致。
    /// </summary>
    private static void ApplyDatabaseType(DbConnectionOptions options, string configId, int fallbackIndex, params string[] environmentVariableNames)
    {
        var value = environmentVariableNames
                    .Select(Environment.GetEnvironmentVariable)
                    .FirstOrDefault(item => !string.IsNullOrWhiteSpace(item));
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var config = FindConfig(options, configId, fallbackIndex);
        if (config != null)
        {
            config.DbType = ParseDbType(value, environmentVariableNames[0]);
        }
    }

    /// <summary>
    /// 按 ConfigId 查找连接项；未命中时回落到指定下标（主库下标为 0），越界返回 null。
    /// </summary>
    private static DbConnectionConfig? FindConfig(DbConnectionOptions options, string configId, int fallbackIndex)
    {
        var config = options.ConnectionConfigs.FirstOrDefault(item => item.ConfigId?.ToString() == configId);
        if (config == null && fallbackIndex >= 0 && fallbackIndex < options.ConnectionConfigs.Count)
        {
            config = options.ConnectionConfigs[fallbackIndex];
        }

        return config;
    }

    /// <summary>
    /// 解析环境变量值为 SqlSugar DbType；无效取值启动期 fail-fast（报错统一带变量名）。
    /// <para>注：必须写 <c>System.Enum</c>——全局 using 引入的 <c>GameFrameX.Core.Enum</c> 命名空间会遮蔽 <c>Enum</c> 类型名。</para>
    /// </summary>
    private static DbType ParseDbType(string value, string environmentVariableName)
    {
        if (!System.Enum.TryParse<DbType>(value, true, out var dbType))
        {
            throw new InvalidOperationException($"环境变量 {environmentVariableName} 的值 \"{value}\" 不是有效的 SqlSugar DbType。");
        }

        return dbType;
    }

    private static bool HasEnvironmentVariable(string name)
    {
        return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name));
    }

    private static bool HasEnvironmentVariablePair(string nameA, string nameB)
    {
        return HasEnvironmentVariable(nameA) && HasEnvironmentVariable(nameB);
    }

    /// <summary>
    /// 校验 DbType 与连接串风格一致，把「半套环境变量覆盖」（只改了类型或只改了连接串）在启动期暴露为异常，
    /// 避免运行到首个数据库访问才报 provider 错误。
    /// ponytail: 仅识别 Sqlite ↔ Host= 串、PostgreSQL ↔ DataSource= 串两种已知部署错配，新增数据库类型时按需扩展。
    /// </summary>
    private static void ValidateProviderConnectionStringConsistency(DbConnectionOptions options)
    {
        foreach (var config in options.ConnectionConfigs)
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
                    "若要通过环境变量切换数据库，GFX_*_DB_TYPE 与 GFX_*_DB_CONNECTION_STRING 必须成对设置。");
            }

            if (config.DbType == DbType.PostgreSQL && isSqliteStyle)
            {
                throw new InvalidOperationException(
                    $"数据库连接配置 {configId} 的 DbType 为 PostgreSQL，但连接串是 DataSource= 风格（\"{config.ConnectionString[..Math.Min(30, config.ConnectionString.Length)]}...\"）。" +
                    "若要通过环境变量切换数据库，GFX_*_DB_TYPE 与 GFX_*_DB_CONNECTION_STRING 必须成对设置。");
            }
        }
    }
}