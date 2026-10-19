using GameFrameX.Core.Const;
using GameFrameX.Core.Option;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;
using Npgsql;
using SqlSugar;
using System.Security.Cryptography;

namespace GameFrameX.Core.Service.Tenant;

public sealed class PostgreSqlTenantDatabaseProvisioner
{
    internal const string TenantDatabasePrefix = "gameframex_admin_tenant_";
    internal const string TenantUserPrefix = "gameframex_admin_tenant_user_";

    /// <summary>
    /// PostgreSQL duplicate_object（role/database 已存在）SQLSTATE；建号竞速吸收判定用。
    /// </summary>
    private const string DuplicateObjectSqlState = "42710";

    public TenantDatabaseProvisioningPlan Provision(long tenantId, DbConnectionConfig mainConfig, DbConnectionConfig provisioningConfig)
    {
        var plan = BuildPlan(tenantId, provisioningConfig, GeneratePassword());

        using (var connection = new NpgsqlConnection(plan.AdminConnectionString))
        {
            connection.Open();

            if (ScalarExists(connection, "SELECT 1 FROM pg_roles WHERE rolname = @name", plan.Username))
            {
                throw new InvalidOperationException($"租户数据库账号 {plan.Username} 已存在，禁止复用既有账号开通租户库。");
            }

            if (ScalarExists(connection, "SELECT 1 FROM pg_database WHERE datname = @name", plan.DatabaseName))
            {
                throw new InvalidOperationException($"租户数据库 {plan.DatabaseName} 已存在，禁止复用既有库开通租户。");
            }
        }

        ExecuteProvisioning(plan, mainConfig);
        return plan;
    }

    /// <summary>
    /// 依据建库专用连接（gfx_provisioner，CREATEROLE+CREATEDB 非 superuser）推导租户库开通计划：
    /// 管理串与租户串的 Host/Port 等参数均继承自该连接（同一 PG 实例），仅库名/账号/密码按租户 ID 派生。
    /// </summary>
    internal static TenantDatabaseProvisioningPlan BuildPlan(long tenantId, DbConnectionConfig provisioningConfig, string password)
    {
        if (tenantId <= 0)
        {
            throw new InvalidOperationException("租户 ID 必须大于 0，才能生成租户库连接。");
        }

        if (provisioningConfig.DbType != DbType.PostgreSQL)
        {
            throw new InvalidOperationException("普通租户库自动创建仅支持 PostgreSQL。");
        }

        if (string.IsNullOrWhiteSpace(provisioningConfig.ConnectionString))
        {
            throw new InvalidOperationException("建库专用 PostgreSQL 连接串缺失，无法生成租户库连接。");
        }

        var adminBuilder = new NpgsqlConnectionStringBuilder(provisioningConfig.ConnectionString);
        var tenantBuilder = new NpgsqlConnectionStringBuilder(provisioningConfig.ConnectionString)
        {
            Database = TenantDatabasePrefix + tenantId,
            Username = TenantUserPrefix + tenantId,
            Password = password,
        };

        return new TenantDatabaseProvisioningPlan(
            tenantId,
            adminBuilder.ConnectionString,
            tenantBuilder.Database,
            tenantBuilder.Username,
            password,
            tenantBuilder.ConnectionString);
    }

    /// <summary>
    /// 以建库专用账号开通租户库（PG16+ 语义，全部语句非幂等，要求 role / database 均不存在——存在性预检由调用方负责）。实测 PG16 语义要点：
    /// <para>① CREATEROLE 账号 CREATE ROLE 后获得的隐式 membership 带 ADMIN OPTION 但 <b>不带 SET</b>，
    /// 而 CREATE DATABASE ... OWNER 与 SET ROLE 都要求对 owner 具备 SET 能力，故须先自授 <c>GRANT ... WITH SET OPTION</c>；
    /// 该隐式行的 grantor 是 bootstrap superuser，provisioner 自己撤不掉，残留会保留 ALTER/DROP/再授权（重新 GRANT SET 即提权）能力，
    /// 因此最后必须经主管理通道再执行一次 REVOKE 清除（主通道为 superuser/隐式行 grantor；失败即抛错，fail-closed）。</para>
    /// <para>② 库级 REVOKE/GRANT 属库主权限：SET ROLE 切租户账号身份后执行 REVOKE CONNECT FROM PUBLIC，收回后平台侧无显式授权的账号均无法连接租户库。</para>
    /// <para>③ PG15+ 新库 public schema 归 pg_database_owner = 库主，无需再改 schema 归属。</para>
    /// <para>最终效果：provisioner 对租户账号的 ALTER / DROP / SET ROLE / 再授权能力全部断绝，也无法连接租户库。</para>
    /// </summary>
    internal static void ExecuteProvisioning(TenantDatabaseProvisioningPlan plan, DbConnectionConfig mainConfig)
    {
        var provisionerUsername = new NpgsqlConnectionStringBuilder(plan.AdminConnectionString).Username;
        using (var connection = new NpgsqlConnection(plan.AdminConnectionString))
        {
            connection.Open();

            ExecuteNonQuery(connection, $"CREATE ROLE {QuoteIdentifier(plan.Username)} WITH LOGIN PASSWORD {QuoteLiteral(plan.Password)}");
            ExecuteNonQuery(connection, $"GRANT {QuoteIdentifier(plan.Username)} TO {QuoteIdentifier(provisionerUsername)} WITH SET OPTION");
            ExecuteNonQuery(connection, $"CREATE DATABASE {QuoteIdentifier(plan.DatabaseName)} OWNER {QuoteIdentifier(plan.Username)} ENCODING 'UTF8'");

            ExecuteNonQuery(connection, $"SET ROLE {QuoteIdentifier(plan.Username)}");
            try
            {
                ExecuteNonQuery(connection, $"REVOKE CONNECT ON DATABASE {QuoteIdentifier(plan.DatabaseName)} FROM PUBLIC");
                ExecuteNonQuery(connection, $"GRANT CONNECT, TEMPORARY ON DATABASE {QuoteIdentifier(plan.DatabaseName)} TO {QuoteIdentifier(plan.Username)}");
            }
            finally
            {
                ExecuteNonQuery(connection, "RESET ROLE");
            }

            // 自断第一步：撤销自授的 SET membership 行（grantor 是 provisioner 自己，可撤）
            ExecuteNonQuery(connection, $"REVOKE {QuoteIdentifier(plan.Username)} FROM {QuoteIdentifier(provisionerUsername)}");
        }

        // 自断第二步：清除 CREATE ROLE 时以 bootstrap superuser 为 grantor 落下的隐式 admin 行（provisioner 通道无权撤销）。
        // 主通道缺权时本句抛错使整个开通失败，绝不静默保留提权后门。
        using (var mainConnection = new NpgsqlConnection(mainConfig.ConnectionString))
        {
            mainConnection.Open();
            ExecuteNonQuery(mainConnection, $"REVOKE {QuoteIdentifier(plan.Username)} FROM {QuoteIdentifier(provisionerUsername)}");
        }
    }

    /// <summary>
    /// 注册租户 PG 角色启动自愈（GFX-727 后遗留形态：角色被清/误删而 system_tenant 注册表仍在，默认租户项 CreateDatabase 以 28P01 裸栈崩溃）。
    /// <para>边界（守住 C317/C318 安全模型，缺一即不再是「正确的自动创建」）：</para>
    /// <para>① 角色缺失才重建；密码取注册表连接串（默认租户取 ConnectionConfigs 权威值，与 InitDefaultTenantDatabase 对账口径一致）
    /// ——让数据库事实对齐注册表，绝不反向改写注册表、绝不轮转密码。</para>
    /// <para>② 建号走 gfx_provisioner 管理通道（CREATEROLE）；缺省回落主库连接并告警（降级模式，与
    /// SystemTenantService.ResolveProvisioningConfig 同口径）。绝不为自愈给运行账号加 CREATEROLE。</para>
    /// <para>③ 角色已存在则一律不动（含密码不匹配场景）——C318 把存量明文凭证视为已泄漏形态，静默重置会掩盖凭证漂移；
    /// 此类漂移走 db/tenant-isolation/02-migrate-existing-tenants.sh 人工管线轮转。</para>
    /// <para>④ 不重开 provisioner 对既有租户角色的管理权：仅清理本次建号落下的隐式 admin 行（自断语义，失败即抛）。</para>
    /// <para>失败口径：默认租户自愈失败抛错阻断启动（文案含恢复路径，远好于后续 28P01 裸栈）；其余租户记入报告由调用方告警，不阻断平台。</para>
    /// <para>幂等：角色在场即跳过；多 host 并发建号竞速由 duplicate_object 捕获吸收（对端已完成建号）。</para>
    /// </summary>
    /// <param name="mainDb">主库连接（读 system_tenant 注册表；目录查询走主库连接，不依赖 provisioning 通道可用性）。</param>
    /// <param name="mainConfig">主库连接配置（目录读取 + 权限矩阵归位 + 自断清理通道）。</param>
    /// <param name="dbOptions">数据库配置选项（provisioning 通道与默认租户权威连接串来源）。</param>
    public static TenantRoleSelfHealReport EnsureRegisteredTenantRoles(ISqlSugarClient mainDb, DbConnectionConfig mainConfig, DbConnectionOptions dbOptions)
    {
        if (mainConfig.DbType != DbType.PostgreSQL || string.IsNullOrWhiteSpace(mainConfig.ConnectionString))
        {
            return TenantRoleSelfHealReport.Empty;
        }

        // 冻结形态部署（EnableInitTable/EnableInitSeed 全关）下注册表可能不存在：无从自愈也不应阻断，直接跳过
        if (!mainDb.DbMaintenance.IsAnyTable("system_tenant_entity"))
        {
            return TenantRoleSelfHealReport.Empty;
        }

        var adminConnectionString = ResolveAdminConnectionString(mainConfig, dbOptions, out var isProvisioningChannel);
        var defaultTenantId = TenantDefaultDataTemplates.MainTenantId;
        var defaultTenantConnection = dbOptions.ConnectionConfigs
                                                .FirstOrDefault(u => u.ConfigId?.ToString() == defaultTenantId.ToString())?
                                                .ConnectionString;
        var tenants = mainDb.Queryable<SystemTenantEntity>()
                            .Filter(null, true)
                            .Where(u => u.IsDeleted == false && u.TenantType == TenantTypeEnum.Db && u.DbType == DbType.PostgreSQL)
                            .OrderBy(u => u.Id)
                            .ToList();
        if (tenants.Count == 0)
        {
            return TenantRoleSelfHealReport.Empty;
        }

        // 阶段一：注册表 → 解密后连接串（默认租户以 ConnectionConfigs 权威值优先，与后续对账写入同源）
        var registrations = new List<TenantRoleRegistration>();
        var failures = new List<string>();
        foreach (var tenant in tenants)
        {
            var isDefaultTenant = tenant.Id == defaultTenantId;
            try
            {
                var connection = isDefaultTenant
                    ? defaultTenantConnection ?? TenantConnectionCrypto.Decrypt(tenant.Connection)
                    : TenantConnectionCrypto.Decrypt(tenant.Connection);
                registrations.Add(new TenantRoleRegistration(tenant.Id, connection, isDefaultTenant));
            }
            catch (Exception exception)
            {
                RecordFailure(failures, isDefaultTenant, $"租户 {tenant.Id} 连接串解密失败：{exception.Message}", exception);
            }
        }

        // 阶段二：目录比对 → 自愈动作规划（角色在场即跳过，不触碰）
        List<TenantRoleHealAction> actions;
        using (var mainConnection = new NpgsqlConnection(mainConfig.ConnectionString))
        {
            mainConnection.Open();
            var existingRoleNames = QueryExistingNames(mainConnection, "SELECT rolname FROM pg_roles");
            var existingDatabaseNames = QueryExistingNames(mainConnection, "SELECT datname FROM pg_database");
            actions = BuildHealActions(registrations, existingRoleNames, existingDatabaseNames, out var invalidRegistrations);
            failures.AddRange(invalidRegistrations);
        }

        // 阶段三：执行（建号走管理通道；既有库的对象移交/库主/REVOKE PUBLIC 走主通道，缺权即抛 fail-closed）
        var healedRoles = new List<string>();
        foreach (var action in actions)
        {
            try
            {
                if (action.RequiresDatabaseCreation)
                {
                    var plan = new TenantDatabaseProvisioningPlan(
                        action.TenantId,
                        adminConnectionString,
                        action.DatabaseName,
                        action.Username,
                        action.Password,
                        action.ConnectionString);
                    ExecuteProvisioning(plan, mainConfig);
                    healedRoles.Add($"{action.TenantId}/{action.Username}（含建库 {action.DatabaseName}）");
                }
                else
                {
                    HealRoleForExistingDatabase(action, mainConfig, adminConnectionString, isProvisioningChannel);
                    healedRoles.Add($"{action.TenantId}/{action.Username}");
                }
            }
            catch (PostgresException exception) when (exception.SqlState == DuplicateObjectSqlState)
            {
                // 多 host 并发建号竞速：对端已建号，本端吸收（对端同步完成权限矩阵，幂等）
                healedRoles.Add($"{action.TenantId}/{action.Username}（并发竞速由对端完成）");
            }
            catch (Exception exception)
            {
                RecordFailure(failures, action.IsDefaultTenant, $"租户 {action.TenantId} 角色 {action.Username} 自愈失败：{exception.Message}", exception);
            }
        }

        return new TenantRoleSelfHealReport(healedRoles, failures);
    }

    /// <summary>
    /// 规划自愈动作（纯函数，供单测注入目录状态验证口径）：角色在场跳过；连接串缺 Username/Password/Database
    /// 或解析失败记为 invalid（不产生动作）；库同样缺失时标记走完整开通。
    /// </summary>
    internal static List<TenantRoleHealAction> BuildHealActions(
        IReadOnlyList<TenantRoleRegistration> registrations,
        IReadOnlySet<string> existingRoleNames,
        IReadOnlySet<string> existingDatabaseNames,
        out IReadOnlyList<string> invalidRegistrations)
    {
        var invalid = new List<string>();
        var actions = new List<TenantRoleHealAction>();
        foreach (var registration in registrations)
        {
            string username;
            string password;
            string databaseName;
            try
            {
                var builder = new NpgsqlConnectionStringBuilder(registration.ConnectionString);
                username = builder.Username;
                password = builder.Password;
                databaseName = builder.Database;
            }
            catch (Exception)
            {
                invalid.Add($"租户 {registration.TenantId} 连接串解析失败，无法自愈（恢复路径：db/tenant-isolation/02-migrate-existing-tenants.sh）。");
                continue;
            }

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(databaseName))
            {
                invalid.Add($"租户 {registration.TenantId} 连接串缺少 Username/Password/Database，无法自愈（恢复路径：db/tenant-isolation/02-migrate-existing-tenants.sh）。");
                continue;
            }

            if (existingRoleNames.Contains(username))
            {
                continue;
            }

            actions.Add(new TenantRoleHealAction(
                registration.TenantId,
                username,
                databaseName,
                password,
                registration.ConnectionString,
                !existingDatabaseNames.Contains(databaseName),
                registration.IsDefaultTenant));
        }

        invalidRegistrations = invalid;
        return actions;
    }

    /// <summary>
    /// 既有库形态的角色重建：建号（管理通道）→ 库内平台账号名下业务对象移交 → 库主移交 →
    /// REVOKE PUBLIC + 显式授权 → 自断清理（步骤口径与 db/tenant-isolation/02 迁移脚本一致）。
    /// </summary>
    private static void HealRoleForExistingDatabase(TenantRoleHealAction action, DbConnectionConfig mainConfig, string adminConnectionString, bool isProvisioningChannel)
    {
        var provisionerUsername = new NpgsqlConnectionStringBuilder(adminConnectionString).Username;
        var platformRoleName = new NpgsqlConnectionStringBuilder(mainConfig.ConnectionString).Username;

        using (var adminConnection = new NpgsqlConnection(adminConnectionString))
        {
            adminConnection.Open();
            ExecuteNonQuery(adminConnection, $"CREATE ROLE {QuoteIdentifier(action.Username)} WITH LOGIN PASSWORD {QuoteLiteral(action.Password)}");
        }

        // 库内对象移交（表/分区表/序列/视图/物化视图/外部表；索引随表自动转移）——不用 REASSIGN OWNED BY：
        // 平台账号通常是 bootstrap superuser，其名下含系统对象，REASSIGN 会因系统对象整体失败（同 02 脚本口径）
        var transferSql =
            "DO $transfer$ DECLARE r record; BEGIN " +
            "FOR r IN SELECT n.nspname AS nsp, c.relname AS rel, c.relkind AS kind FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace " +
            $"WHERE n.nspname NOT IN ('pg_catalog','information_schema','pg_toast') AND n.nspname NOT LIKE 'pg\\_%' " +
            $"AND pg_get_userbyid(c.relowner) = {QuoteLiteral(platformRoleName)} AND c.relkind IN ('r','p','S','v','m','f') LOOP " +
            $"IF r.kind = 'S' THEN EXECUTE format('ALTER SEQUENCE %I.%I OWNER TO %I', r.nsp, r.rel, {QuoteLiteral(action.Username)}); " +
            $"ELSE EXECUTE format('ALTER TABLE %I.%I OWNER TO %I', r.nsp, r.rel, {QuoteLiteral(action.Username)}); END IF; " +
            "END LOOP; END $transfer$;";
        var tenantDatabaseConnectionString = new NpgsqlConnectionStringBuilder(mainConfig.ConnectionString)
        {
            Database = action.DatabaseName,
        }.ConnectionString;
        using (var tenantConnection = new NpgsqlConnection(tenantDatabaseConnectionString))
        {
            tenantConnection.Open();
            ExecuteNonQuery(tenantConnection, transferSql);

            // 移交后就地断言：业务 schema 内平台账号名下对象残留必须为 0，非 0 即数据形态异常，不盲目放行
            var residualCount = QueryScalarLong(tenantConnection,
                "SELECT count(*) FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace " +
                "WHERE n.nspname NOT IN ('pg_catalog','information_schema','pg_toast') AND n.nspname NOT LIKE 'pg\\_%' " +
                $"AND pg_get_userbyid(c.relowner) = {QuoteLiteral(platformRoleName)} AND c.relkind IN ('r','p','S','v','m','f')");
            if (residualCount != 0)
            {
                throw new InvalidOperationException($"租户库 {action.DatabaseName} 内仍有 {residualCount} 个对象属于 {platformRoleName}，无法完成移交。");
            }
        }

        using (var mainConnection = new NpgsqlConnection(mainConfig.ConnectionString))
        {
            mainConnection.Open();
            ExecuteNonQuery(mainConnection, $"ALTER DATABASE {QuoteIdentifier(action.DatabaseName)} OWNER TO {QuoteIdentifier(action.Username)}");
            ExecuteNonQuery(mainConnection, $"REVOKE CONNECT ON DATABASE {QuoteIdentifier(action.DatabaseName)} FROM PUBLIC");
            ExecuteNonQuery(mainConnection, $"GRANT CONNECT, TEMPORARY ON DATABASE {QuoteIdentifier(action.DatabaseName)} TO {QuoteIdentifier(action.Username)}");

            if (isProvisioningChannel)
            {
                // 自断：CREATE ROLE 经 CREATEROLE 通道落下以 bootstrap superuser 为 grantor 的隐式 admin 行，
                // 主通道（superuser/隐式行 grantor）撤除；失败即抛，绝不静默保留 provisioner 对租户角色的提权能力
                ExecuteNonQuery(mainConnection, $"REVOKE {QuoteIdentifier(action.Username)} FROM {QuoteIdentifier(provisionerUsername)}");
            }
        }
    }

    /// <summary>
    /// 解析建号管理通道：显式配置的 gfx_provisioner 优先；缺省回落主库连接（降级模式，同 SystemTenantService 口径）。
    /// </summary>
    private static string ResolveAdminConnectionString(DbConnectionConfig mainConfig, DbConnectionOptions dbOptions, out bool isProvisioningChannel)
    {
        if (!string.IsNullOrWhiteSpace(dbOptions.ProvisioningConnectionString))
        {
            isProvisioningChannel = true;
            return dbOptions.ProvisioningConnectionString;
        }

        isProvisioningChannel = false;
        Console.WriteLine("[Warning] DbConnection.ProvisioningConnectionString 未配置，注册租户 PG 角色自愈回落使用主库连接；建议配置 gfx_provisioner 专用账号（CREATEROLE+CREATEDB 非 superuser）分离建库通道。");
        return mainConfig.ConnectionString;
    }

    /// <summary>
    /// 失败记录口径：默认租户失败抛错阻断启动（后续 CreateDatabase 必然 28P01，可执行文案远好于裸栈）；其余租户进报告。
    /// </summary>
    private static void RecordFailure(List<string> failures, bool isDefaultTenant, string message, Exception exception)
    {
        const string recovery = "恢复路径：db/tenant-isolation/02-migrate-existing-tenants.sh（或重建角色后重启）。";
        if (isDefaultTenant)
        {
            throw new InvalidOperationException($"{message}；{recovery}", exception);
        }

        failures.Add($"{message}；{recovery}");
    }

    private static HashSet<string> QueryExistingNames(NpgsqlConnection connection, string sql)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        using (var command = new NpgsqlCommand(sql, connection))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var name = reader.IsDBNull(0) ? null : reader.GetString(0);
                if (!string.IsNullOrEmpty(name))
                {
                    names.Add(name);
                }
            }
        }

        return names;
    }

    private static long QueryScalarLong(NpgsqlConnection connection, string sql)
    {
        using (var command = new NpgsqlCommand(sql, connection))
        {
            return Convert.ToInt64(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    private static bool ScalarExists(NpgsqlConnection connection, string sql, string value)
    {
        using (var command = new NpgsqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("name", value);
            return command.ExecuteScalar() != null;
        }
    }

    private static void ExecuteNonQuery(NpgsqlConnection connection, string sql)
    {
        using (var command = new NpgsqlCommand(sql, connection))
        {
            command.ExecuteNonQuery();
        }
    }

    private static string QuoteIdentifier(string value)
    {
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    private static string QuoteLiteral(string value)
    {
        return "'" + value.Replace("'", "''") + "'";
    }

    private static string GeneratePassword()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();
    }
}

/// <summary>
/// 注册租户自愈输入：租户 ID + 解密后的连接串（<see cref="BuildHealActions"/> 内解析账号/密码/库名）。
/// </summary>
/// <param name="TenantId">租户 ID。</param>
/// <param name="ConnectionString">解密后的租户库连接串。</param>
/// <param name="IsDefaultTenant">是否默认租户。</param>
internal sealed record TenantRoleRegistration(long TenantId, string? ConnectionString, bool IsDefaultTenant);
