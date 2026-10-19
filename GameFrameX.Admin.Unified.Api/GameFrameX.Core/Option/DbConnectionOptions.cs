using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Extension.Option;
using GameFrameX.Core.Service.Tenant;
using MySqlConnector;
using Npgsql;

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

    /// <summary>
    /// 租户建库专用连接串（gfx_provisioner 账号，CREATEROLE+CREATEDB 非 superuser）。
    /// <para>配置节 <c>DbConnection:ProvisioningConnectionString</c>；缺省回落主库连接并在开通租户时告警（降级模式），
    /// 建库通道与业务主库凭据分离后，主库账号被拖库不再连带暴露建库权限。</para>
    /// </summary>
    public string? ProvisioningConnectionString { get; set; }

    public void PostConfigure(DbConnectionOptions options, IConfiguration configuration)
    {
        ApplyEnvironmentOverrides(options);

        foreach (var dbConfig in options.ConnectionConfigs)
        {
            if (string.IsNullOrWhiteSpace(dbConfig.ConfigId?.ToString()))
            {
                dbConfig.ConfigId = SqlSugarConst.MainConfigId;
            }
        }

        DeriveDefaultTenantDatabase(options);

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

        ApplyDatabaseType(options, TenantDefaultDataTemplates.MainTenantId.ToString(), 1,
                          "GFX_DEFAULT_TENANT_DB_TYPE");

        ApplyConnectionString(options, SqlSugarConst.MainConfigId, 0,
                              "GFX_MAIN_DB_CONNECTION_STRING",
                              "MAIN_DB_CONNECTION_STRING",
                              "ConnectionString");

        ApplyConnectionString(options, TenantDefaultDataTemplates.MainTenantId.ToString(), 1,
                              "GFX_DEFAULT_TENANT_DB_CONNECTION_STRING",
                              "DEFAULT_TENANT_DB_CONNECTION_STRING");

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
    /// 按 ConfigId 查找连接项；未命中时回落到指定下标（历史文件顺序约定：0=main、1=默认租户），越界返回 null。
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

    /// <summary>
    /// 默认租户库自动推导：文件与环境变量均未显式配置默认租户库时，从主库连接串推导其连接。
    /// <para>显式优先顺序：文件存在 ConfigId=<see cref="TenantDefaultDataTemplates.MainTenantId"/> 项（由覆盖逻辑改写文件项）、
    /// 或 GFX_DEFAULT_TENANT_DB_TYPE / GFX_DEFAULT_TENANT_DB_CONNECTION_STRING 成对设置时，跳过推导——
    /// 后者在文件省略该项时由环境变量对补建连接项（半套设置按「必须成对」fail-fast）。</para>
    /// <para>仅支持 PostgreSQL、Sqlite、MySql 与 MySqlConnector：PostgreSQL / MySQL 复用普通租户建库的「前缀+租户ID」命名规律
    ///（Server/Port/账号/密码等参数全继承主库），其余 DbType 无推导先例，启动抛错要求显式配置，
    /// 对齐普通租户库 TenantDbTypeForbidden 的收紧精神；Odbc / Custom 连接串语义自定义，永不支持推导。</para>
    /// </summary>
    private static void DeriveDefaultTenantDatabase(DbConnectionOptions options)
    {
        var tenantConfigId = TenantDefaultDataTemplates.MainTenantId.ToString();
        if (options.ConnectionConfigs.Any(item => item.ConfigId?.ToString() == tenantConfigId))
        {
            return;
        }

        var hasTenantDbType = HasEnvironmentVariable("GFX_DEFAULT_TENANT_DB_TYPE");
        var hasTenantConnectionString = HasEnvironmentVariable("GFX_DEFAULT_TENANT_DB_CONNECTION_STRING");
        if (hasTenantDbType || hasTenantConnectionString)
        {
            if (hasTenantDbType && hasTenantConnectionString)
            {
                // 成对显式变量按「文件可省略该项」口径补建：覆盖逻辑（ApplyDatabaseType / ApplyConnectionString）只改已有项，
                // 文件缺项时静默落空，推导又在此被跳过，最终种子期报「默认租户库连接配置缺失」启动崩溃
                options.ConnectionConfigs.Add(CreateDefaultTenantConfig(
                                                 ParseDbType(Environment.GetEnvironmentVariable("GFX_DEFAULT_TENANT_DB_TYPE")!, "GFX_DEFAULT_TENANT_DB_TYPE"),
                                                 Environment.GetEnvironmentVariable("GFX_DEFAULT_TENANT_DB_CONNECTION_STRING")!));
                return;
            }

            // 半套显式变量（只设其一）跳过推导后，错误会后移到 InitDefaultTenantDatabase 的「配置缺失」文案，误导改文件而非补变量——
            // 在此按成对口径直接 fail-fast，与 GFX_MAIN_DB_* 一致
            throw new InvalidOperationException("GFX_DEFAULT_TENANT_DB_TYPE 与 GFX_DEFAULT_TENANT_DB_CONNECTION_STRING 必须成对设置。");
        }

        var mainConfig = options.ConnectionConfigs.FirstOrDefault(item => item.ConfigId?.ToString() == SqlSugarConst.MainConfigId);
        if (mainConfig == null || string.IsNullOrWhiteSpace(mainConfig.ConnectionString))
        {
            return;
        }

        string tenantConnectionString;
        if (mainConfig.DbType == DbType.PostgreSQL)
        {
            tenantConnectionString = DerivePostgreSqlTenantConnectionString(mainConfig.ConnectionString);
        }
        else if (mainConfig.DbType == DbType.Sqlite)
        {
            tenantConnectionString = DeriveSqliteTenantConnectionString(mainConfig.ConnectionString);
        }
        else if (mainConfig.DbType == DbType.MySql || mainConfig.DbType == DbType.MySqlConnector)
        {
            tenantConnectionString = DeriveMySqlTenantConnectionString(mainConfig.ConnectionString);
        }
        else
        {
            throw new InvalidOperationException(
                $"主库 DbType 为 {mainConfig.DbType}，默认租户库不支持自动推导（仅支持 PostgreSQL / Sqlite / MySql / MySqlConnector）；" +
                $"请显式配置 ConfigId={tenantConfigId} 的连接项，或成对设置 GFX_DEFAULT_TENANT_DB_TYPE / GFX_DEFAULT_TENANT_DB_CONNECTION_STRING。");
        }

        // 推导项开关与历史文件第二项等价：EnableInitDb=true 供专用管线幂等建库；
        // 建表/种子开关全关（默认租户走 InitTenantDatabase + DefaultTenantDatabaseSeeder 专用管线，防通用管线重复建）；
        // EnableDiffLog=false 与专用管线的强制值一致
        options.ConnectionConfigs.Add(CreateDefaultTenantConfig(mainConfig.DbType, tenantConnectionString));
    }

    /// <summary>
    /// 构建默认租户库连接项：推导项与成对环境变量补建项共用，开关与历史文件第二项等价——
    /// EnableInitDb=true 供专用管线幂等建库；建表/种子开关全关（默认租户走 InitTenantDatabase + DefaultTenantDatabaseSeeder
    /// 专用管线，防通用管线重复建）；EnableDiffLog=false 与专用管线的强制值一致。
    /// </summary>
    private static DbConnectionConfig CreateDefaultTenantConfig(DbType dbType, string connectionString)
    {
        return new DbConnectionConfig
        {
            ConfigId = TenantDefaultDataTemplates.MainTenantId.ToString(),
            DbType = dbType,
            ConnectionString = connectionString,
            DbSettings = new DbSettings { EnableInitDb = true, EnableDiffLog = false, },
            TableSettings = new TableSettings { EnableInitTable = false, EnableIncreTable = false, },
            SeedSettings = new SeedSettings { EnableInitSeed = false, EnableIncreSeed = false, },
        };
    }

    /// <summary>
    /// PostgreSQL 推导：主库连接串仅替换库名为「租户库前缀+默认租户ID」（与普通租户库完全同规律），Host/Port/账号/密码全继承。
    /// </summary>
    private static string DerivePostgreSqlTenantConnectionString(string mainConnectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(mainConnectionString)
        {
            Database = PostgreSqlTenantDatabaseProvisioner.TenantDatabasePrefix + TenantDefaultDataTemplates.MainTenantId,
        };
        return builder.ConnectionString;
    }

    /// <summary>
    /// MySQL 推导：主库连接串仅替换库名为「租户库前缀+默认租户ID」（与普通租户库完全同规律），Server/Port/账号/密码等其余参数全继承。
    /// <para>MySql 与 MySqlConnector 两个 DbType 共用 MySqlConnector 的 builder 改写：两者连接串语法兼容（键大小写与 Server/Host、Uid/User ID 等
    /// 别名均被 builder 识别），且 builder 仅做字符串解析不建立连接。注意 SqlSugarCore 5.1.4.216 在 .NET 上仅 DbType.MySql 可真实连接
    ///（驱动即 MySqlConnector 包），DbType.MySqlConnector 的 provider 反射映射损坏，推导/改写层两值均支持但后者仅静态覆盖。</para>
    /// </summary>
    private static string DeriveMySqlTenantConnectionString(string mainConnectionString)
    {
        var builder = new MySqlConnectionStringBuilder(mainConnectionString)
        {
            Database = PostgreSqlTenantDatabaseProvisioner.TenantDatabasePrefix + TenantDefaultDataTemplates.MainTenantId,
        };
        return builder.ConnectionString;
    }

    /// <summary>
    /// Sqlite 推导：主库文件同目录，文件名与 PostgreSQL 库名完全同名（前缀+默认租户ID，仅差扩展名），
    /// 连接串中除 DataSource 外的其余参数原样保留。
    /// </summary>
    private static string DeriveSqliteTenantConnectionString(string mainConnectionString)
    {
        var segments = mainConnectionString.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
        for (var i = 0; i < segments.Count; i++)
        {
            var separatorIndex = segments[i].IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = segments[i][..separatorIndex].Trim();
            if (!string.Equals(key, "DataSource", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(key, "Data Source", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            segments[i] = key + "=" + DeriveSqliteTenantFileName(segments[i][(separatorIndex + 1)..].Trim());
        }

        return string.Join(";", segments) + ";";
    }

    private static string DeriveSqliteTenantFileName(string dataSource)
    {
        var directory = Path.GetDirectoryName(dataSource);
        var tenantFileName = PostgreSqlTenantDatabaseProvisioner.TenantDatabasePrefix
                             + TenantDefaultDataTemplates.MainTenantId
                             + Path.GetExtension(dataSource);
        return string.IsNullOrWhiteSpace(directory) ? tenantFileName : Path.Combine(directory, tenantFileName);
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