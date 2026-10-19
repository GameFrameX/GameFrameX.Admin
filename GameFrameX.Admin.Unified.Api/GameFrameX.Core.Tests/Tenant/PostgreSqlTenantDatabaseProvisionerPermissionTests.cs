using GameFrameX.Core.Option;
using GameFrameX.Core.Service.Tenant;
using Npgsql;
using SqlSugar;
using Xunit.Abstractions;

namespace GameFrameX.Core.Tests.Tenant;

// C317（GFX-726）：provisioner 独立账号自断的本地 PG 16 集成验证。
// change：gfx-doc/dev/tenant/changes/C317-gfx-726-tenant-db-isolation-hardening/change.md
// 验收映射（issue 验收第 1、3 条）：
//   1. gfx_provisioner（NOSUPERUSER+CREATEROLE+CREATEDB）完整建库流程后——
//      DROP ROLE / DROP DATABASE / SET ROLE / 平台账号 CONNECT tenant_db 四项均权限拒绝；
//      另外验证重新 GRANT SET（提权回径）也被拒（notes.md 迭代 2 的残留后门必须堵死）。
//   2. 租户账号可凭 plan.TenantConnectionString 正常连接新库并在 public 建表（建租户全流程回归的库层前提）。
// 「主库账号」按非 superuser 平台账号口径验证（superuser 在 PG 中天然绕过权限检查，降权属 P2 运维阶段；
// 测试用 provisioner 自身 + 新建普通账号 gfx_platform_plain_perm_test 双重覆盖）。
// PG 不可达时输出 [SKIP] 并直接返回（沿用 SqlSugarDiffLogTenantPrefixTests 模式）。
public sealed class PostgreSqlTenantDatabaseProvisionerPermissionTests
{
    private const long TestTenantId = 997260001;
    private const string ProvisionerUsername = "gfx_provisioner_perm_test";
    private const string ProvisionerPassword = "ProvPermTest@2026";
    private const string PlainPlatformUsername = "gfx_platform_plain_perm_test";
    private const string PlainPlatformPassword = "PlainPermTest@2026";
    private const string InsufficientPrivilegeSqlState = "42501";

    private static readonly string[] SuperuserCandidates =
    {
        // 部署环境显式指定优先
        Environment.GetEnvironmentVariable("GFX_TEST_PG_SUPERUSER_CONNECTION") ?? string.Empty,
        // docker-compose gameframex-postgres 默认（POSTGRES_USER=gameframex）
        "Host=127.0.0.1;Port=5432;Database=gameframex_admin;Username=gameframex;Password=GameFrameX@Local2026;Connection Timeout=3;",
        // 历史测试镜像默认（postgres/postgres）
        "Host=127.0.0.1;Port=5432;Database=postgres;Username=postgres;Password=postgres;Connection Timeout=3;",
    };

    private readonly ITestOutputHelper _output;

    public PostgreSqlTenantDatabaseProvisionerPermissionTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Provision_WithDedicatedProvisionerAccount_SelfRevokesAllPowers()
    {
        var superuserConnectionString = TryConnectSuperuser();
        if (superuserConnectionString == null)
        {
            return;
        }

        Cleanup(superuserConnectionString);

        try
        {
            // ── fixture：以 superuser 建独立 provisioner（NOSUPERUSER+CREATEROLE+CREATEDB）与普通平台账号 ──
            Execute(superuserConnectionString, $"CREATE ROLE {Quote(ProvisionerUsername)} WITH LOGIN PASSWORD {Literal(ProvisionerPassword)} NOSUPERUSER CREATEROLE CREATEDB");
            Execute(superuserConnectionString, $"CREATE ROLE {Quote(PlainPlatformUsername)} WITH LOGIN PASSWORD {Literal(PlainPlatformPassword)}");
            var provisioningConnectionString = new NpgsqlConnectionStringBuilder(superuserConnectionString)
            {
                Username = ProvisionerUsername,
                Password = ProvisionerPassword,
            }.ConnectionString;

            // ── 完整开通流程（走真实 Provision：BuildPlan + EnsureDatabaseAndRole 双通道自断）──
            var provisioner = new PostgreSqlTenantDatabaseProvisioner();
            var mainConfig = new DbConnectionConfig { DbType = DbType.PostgreSQL, ConnectionString = superuserConnectionString, };
            var provisioningConfig = new DbConnectionConfig { DbType = DbType.PostgreSQL, ConnectionString = provisioningConnectionString, };
            var plan = provisioner.Provision(TestTenantId, mainConfig, provisioningConfig);

            Assert.Equal(PostgreSqlTenantDatabaseProvisioner.TenantDatabasePrefix + TestTenantId, plan.DatabaseName);

            // ── 租户账号可正常连接新库并建表（建租户全流程的库层前提）──
            using (var tenantConnection = new NpgsqlConnection(plan.TenantConnectionString))
            {
                tenantConnection.Open();
                ExecuteOn(tenantConnection, "CREATE TABLE permission_smoke_check (id bigint primary key)");
                ExecuteOn(tenantConnection, "DROP TABLE permission_smoke_check");
            }

            // ── 自断断言 1：membership 行清零 ──
            using (var superuserConnection = new NpgsqlConnection(superuserConnectionString))
            {
                superuserConnection.Open();
                using (var command = new NpgsqlCommand(
                           "SELECT count(*) FROM pg_auth_members m JOIN pg_roles r ON r.oid = m.member JOIN pg_roles t ON t.oid = m.roleid WHERE r.rolname = @member AND t.rolname = @role",
                           superuserConnection))
                {
                    command.Parameters.AddWithValue("member", ProvisionerUsername);
                    command.Parameters.AddWithValue("role", plan.Username);
                    Assert.Equal(0L, (long)command.ExecuteScalar()!);
                }
            }

            // ── 自断断言 2：四项能力全部权限拒绝（以 provisioner 身份执行）──
            AssertPermissionDenied(provisioningConnectionString, $"SET ROLE {Quote(plan.Username)}", openDatabase: null);
            AssertPermissionDenied(provisioningConnectionString, $"DROP ROLE {Quote(plan.Username)}", openDatabase: null);
            AssertPermissionDenied(provisioningConnectionString, $"DROP DATABASE {Quote(plan.DatabaseName)}", openDatabase: null);
            // 提权回径：重新 GRANT SET OPTION 也必须被拒（notes.md 迭代 2 的残留后门）
            AssertPermissionDenied(provisioningConnectionString, $"GRANT {Quote(plan.Username)} TO {Quote(ProvisionerUsername)} WITH SET OPTION", openDatabase: null);

            // ── 自断断言 3：平台侧账号 CONNECT tenant_db 全部拒绝 ──
            AssertPermissionDenied(provisioningConnectionString, "SELECT 1", openDatabase: plan.DatabaseName);
            var plainPlatformConnectionString = new NpgsqlConnectionStringBuilder(superuserConnectionString)
            {
                Username = PlainPlatformUsername,
                Password = PlainPlatformPassword,
            }.ConnectionString;
            AssertPermissionDenied(plainPlatformConnectionString, "SELECT 1", openDatabase: plan.DatabaseName);
        }
        finally
        {
            Cleanup(superuserConnectionString);
        }
    }

    private string? TryConnectSuperuser()
    {
        foreach (var candidate in SuperuserCandidates.Where(u => !string.IsNullOrWhiteSpace(u)))
        {
            try
            {
                using (var connection = new NpgsqlConnection(candidate))
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand("SELECT setting FROM pg_settings WHERE name = 'server_version'", connection))
                    {
                        _output.WriteLine($"[PG] 可用：{command.ExecuteScalar()}（{MaskHost(candidate)}）");
                    }
                }

                return candidate;
            }
            catch (Exception exception)
            {
                _output.WriteLine($"[SKIP] 候选连接不可用（{exception.Message}）");
            }
        }

        _output.WriteLine("[SKIP] 本地 postgres 不可用，跳过 provisioner 自断集成验证。");
        return null;
    }

    private static void Cleanup(string superuserConnectionString)
    {
        var databaseName = PostgreSqlTenantDatabaseProvisioner.TenantDatabasePrefix + TestTenantId;
        var tenantRoleName = PostgreSqlTenantDatabaseProvisioner.TenantUserPrefix + TestTenantId;
        var ignoreNotFound = new[] { $"DROP DATABASE IF EXISTS {Quote(databaseName)} WITH (FORCE)", $"DROP ROLE IF EXISTS {Quote(tenantRoleName)}", $"DROP ROLE IF EXISTS {Quote(ProvisionerUsername)}", $"DROP ROLE IF EXISTS {Quote(PlainPlatformUsername)}", };
        foreach (var sql in ignoreNotFound)
        {
            Execute(superuserConnectionString, sql);
        }
    }

    private static void Execute(string connectionString, string sql)
    {
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            ExecuteOn(connection, sql);
        }
    }

    private static void ExecuteOn(NpgsqlConnection connection, string sql)
    {
        using (var command = new NpgsqlCommand(sql, connection))
        {
            command.ExecuteNonQuery();
        }
    }

    private void AssertPermissionDenied(string connectionString, string sql, string? openDatabase)
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            if (openDatabase != null)
            {
                builder.Database = openDatabase;
            }

            using (var connection = new NpgsqlConnection(builder.ConnectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        catch (PostgresException exception) when (exception.SqlState == InsufficientPrivilegeSqlState)
        {
            _output.WriteLine($"[OK] 权限拒绝符合预期：{sql} → {exception.MessageText}");
            return;
        }

        Assert.Fail($"预期权限拒绝（42501）但语句成功执行：database={openDatabase ?? "(default)"} sql={sql}");
    }

    private static string Quote(string identifier)
    {
        return "\"" + identifier.Replace("\"", "\"\"") + "\"";
    }

    private static string Literal(string value)
    {
        return "'" + value.Replace("'", "''") + "'";
    }

    private static string MaskHost(string connectionString)
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            return $"{builder.Host}:{builder.Port}/{builder.Database}";
        }
        catch (Exception)
        {
            return "(unparsable)";
        }
    }
}
