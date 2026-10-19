using GameFrameX.Core.Option;
using GameFrameX.Core.SqlSugar;
using Npgsql;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

/// <summary>
/// 真实 PostgreSQL 测试库生命周期：CREATE DATABASE 唯一库 → 测试 → DROP DATABASE WITH (FORCE) 回收。
/// <para>背景：SQLite provider 的 CodeFirst 行为与生产 PostgreSQL 不一致——存量表静默不加列、
/// <c>DefaultValue</c> 不进建表 DDL。SQLite 单测验证不了 schema 演化（2026-09-11 password_configured 23502
/// 事故根因之一：全部 SQLite 测试绿、真实 PG 一启动即炸）。DDL 行为测试必须连真实 PostgreSQL。</para>
/// <para>连接串：环境变量 <c>GFX_TEST_PG_CONNECTION_STRING</c> 覆盖，缺省指向本地 docker
/// <c>gameframex-postgres</c> 的维护库（postgres）。PG 不可达时 <see cref="TryCreate"/> 返回 false，
/// 调用方 <c>Assert.Skip</c> 黄跳过（CI 未配 PG service 时不阻塞），本地常驻 PG 环境真实执行。</para>
/// </summary>
public sealed class PostgresTestDatabase : IDisposable
{
    /// <summary>PG 维护库连接串的环境变量名（指向 postgres 维护库，用于建/删测试库）。</summary>
    public const string ConnectionStringEnvironmentVariable = "GFX_TEST_PG_CONNECTION_STRING";

    private const string DefaultMaintainConnectionString = "Host=localhost;Port=5432;Database=postgres;Username=gameframex;Password=GameFrameX@Local2026";

    private readonly string _maintainConnectionString;

    /// <summary>测试库名（gfx_test_ 前缀 + Guid，唯一隔离）。</summary>
    public string DatabaseName { get; }

    /// <summary>指向测试库的连接串（测试内 SqlSugar 连接用）。</summary>
    public string ConnectionString { get; }

    private PostgresTestDatabase(string databaseName, string maintainConnectionString)
    {
        DatabaseName = databaseName;
        _maintainConnectionString = maintainConnectionString;
        ConnectionString = $"Host=localhost;Port=5432;Database={databaseName};Username=gameframex;Password=GameFrameX@Local2026";
    }

    /// <summary>
    /// 尝试创建测试库。PG 不可达时返回 false（<paramref name="unavailableReason"/> 供 Assert.Skip），
    /// 不抛异常——PG 可达性属环境前置条件而非被测行为。
    /// </summary>
    public static bool TryCreate(out PostgresTestDatabase? database, out string unavailableReason)
    {
        var maintainConnectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(maintainConnectionString))
        {
            maintainConnectionString = DefaultMaintainConnectionString;
        }

        try
        {
            using (var connection = new NpgsqlConnection(maintainConnectionString))
            {
                connection.Open();
            }
        }
        catch (Exception e)
        {
            database = null;
            unavailableReason = "PostgreSQL 不可达（" + e.Message + "）。DDL 行为测试必须连真实 PG"
                + "（SQLite 不加列、丢默认值，测不出 23502 类问题）。可启动本地 docker gameframex-postgres"
                + " 或设置 " + ConnectionStringEnvironmentVariable + "。";
            return false;
        }

        var databaseName = "gfx_test_" + Guid.NewGuid().ToString("N");
        ExecuteNonQuery(maintainConnectionString, "CREATE DATABASE \"" + databaseName + "\"");
        database = new PostgresTestDatabase(databaseName, maintainConnectionString);
        unavailableReason = string.Empty;
        return true;
    }

    /// <summary>
    /// 构建指向测试库的 SqlSugar 连接配置。必须经 <see cref="SqlSugarSetup.SetDbConfig"/> 装配
    /// ConfigureExternalServices（实体列名 snake_case 映射等），与生产启动链路一致，否则 ORM 读写列映射错位。
    /// </summary>
    /// <param name="configId">连接标识（main 模拟主库）。</param>
    /// <param name="enableSeed">是否执行种子（schema 演化测试关闭，聚焦 DDL）。</param>
    public DbConnectionConfig BuildDbConfig(string configId, bool enableSeed)
    {
        var config = new DbConnectionConfig
        {
            ConfigId = configId,
            DbType = DbType.PostgreSQL,
            ConnectionString = ConnectionString,
            IsAutoCloseConnection = true,
            DbSettings = new DbSettings
            {
                // 测试库已由本类创建，跳过 CreateDatabase
                EnableInitDb = false,
            },
            TableSettings = new TableSettings
            {
                EnableInitTable = true,
                EnableIncreTable = false,
            },
            SeedSettings = new SeedSettings
            {
                EnableInitSeed = enableSeed,
                EnableIncreSeed = false,
            },
        };
        SqlSugarSetup.SetDbConfig(config);
        return config;
    }

    public void Dispose()
    {
        // WITH (FORCE) 先断开残留连接再删库（PG 13+；SqlSugarScope 连接池可能仍持有会话）
        ExecuteNonQuery(_maintainConnectionString, "DROP DATABASE IF EXISTS \"" + DatabaseName + "\" WITH (FORCE)");
    }

    private static void ExecuteNonQuery(string connectionString, string sql)
    {
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            using (var command = new NpgsqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
