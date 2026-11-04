using GameFrameX.Entity.Tenant.ClientApiConfig;
using GameFrameX.Entity.Tenant.Game;
using Npgsql;
using SqlSugar;

namespace GameFrameX.DatabaseMigration;

internal static class PostgresMigrationSmokeTests
{
    internal static void Run(string maintenanceConnection)
    {
        var suffix = Guid.NewGuid().ToString("N");
        var sourceName = "gfx_c400_fix_source_" + suffix;
        var targetName = "gfx_c400_fix_target_" + suffix;
        using (var maintenance = SingleMainMigration.Connect(DbType.PostgreSQL, maintenanceConnection))
        {
            try
            {
                maintenance.Ado.ExecuteCommand($"CREATE DATABASE \"{sourceName}\"");
                maintenance.Ado.ExecuteCommand($"CREATE DATABASE \"{targetName}\"");
                using (var source = SingleMainMigration.Connect(DbType.PostgreSQL, new NpgsqlConnectionStringBuilder(maintenanceConnection) { Database = sourceName }.ConnectionString))
                using (var target = SingleMainMigration.Connect(DbType.PostgreSQL, new NpgsqlConnectionStringBuilder(maintenanceConnection) { Database = targetName }.ConnectionString))
                {
                    var types = new[] { typeof(TenantGameHomeOnlineSampleEntity), typeof(TenantClientApiPolicyEntity) };
                    target.Ado.ExecuteCommand("CREATE TABLE system_tenant_entity (id BIGINT PRIMARY KEY, is_deleted BOOLEAN)");
                    target.Ado.ExecuteCommand("INSERT INTO system_tenant_entity VALUES (1300000000001, false)");
                    source.CodeFirst.InitTables<TenantGameHomeOnlineSampleEntity>();
                    source.Insertable(new TenantGameHomeOnlineSampleEntity { Id = 7001, SampleTime = 1000, OnlineCount = 42 }).ExecuteCommand();
                    source.Ado.ExecuteCommand("ALTER TABLE tenant_game_home_online_sample_entity ADD COLUMN tenant_id BIGINT DEFAULT 1300000000001");
                    source.Ado.ExecuteCommand("ALTER TABLE tenant_game_home_online_sample_entity ADD COLUMN app_id BIGINT DEFAULT 100");
                    source.Ado.ExecuteCommand("CREATE TABLE tenant_game_app_entity (id BIGINT PRIMARY KEY, is_deleted BOOLEAN, signature_required BOOLEAN, device_proof_required BOOLEAN, risk_policy TEXT)");
                    source.Ado.ExecuteCommand("INSERT INTO tenant_game_app_entity VALUES (8101, false, true, false, NULL)");
                    source.CodeFirst.InitTables<TenantClientApiPolicyEntity>();
                    source.Insertable(new TenantClientApiPolicyEntity { Id = 9101, IsEnabled = true, SigningKey = "postgres-test-signing-key" }).ExecuteCommand();
                    foreach (var column in new[] { "signature_required", "device_proof_required", "risk_policy" })
                    {
                        source.Ado.ExecuteCommand($"ALTER TABLE tenant_client_api_policy_entity DROP COLUMN {column}");
                    }

                    SingleMainMigration.Run(source, target, true, types);
                    Require(!target.DbMaintenance.IsAnyTable("tenant_game_home_online_sample_entity", false), "PostgreSQL dry-run 建表/复制必须全部回滚");
                    SingleMainMigration.Run(source, target, false, types);
                    SingleMainMigration.Run(source, target, false, types);

                    LogCredentialCleanupSmokeTests.Run(source, target);
                    Require(target.Ado.GetInt("SELECT COUNT(*) FROM tenant_game_home_online_sample_entity WHERE online_count = 42 AND update_time IS NULL AND deleted_id IS NULL") == 1, "PostgreSQL 非文本NULL必须保真复制且重跑幂等");
                    Require(target.Ado.GetInt("SELECT COUNT(*) FROM tenant_client_api_policy_entity WHERE signature_required = true AND device_proof_required = false AND risk_policy IS NULL") == 1, "PostgreSQL 风险三字段必须保真");
                    Require(source.Ado.GetInt("SELECT COUNT(*) FROM tenant_game_home_online_sample_entity WHERE tenant_id = 1300000000001 AND app_id = 100") == 1, "PostgreSQL 源库保持不变");
                    target.Ado.ExecuteCommand("UPDATE tenant_game_home_online_sample_entity SET online_count = 99");
                    try
                    {
                        SingleMainMigration.Run(source, target, false, types);
                        throw new InvalidOperationException("PostgreSQL 主键冲突必须拒绝");
                    }
                    catch (InvalidOperationException exception) when (exception.Message.Contains("主键碰撞", StringComparison.Ordinal))
                    {
                        Require(target.Ado.GetInt("SELECT online_count FROM tenant_game_home_online_sample_entity") == 99, "PostgreSQL 冲突不得覆盖");
                    }
                }

                Console.WriteLine("PostgreSQL 迁移自测通过：NULL类型绑定、dry-run回滚、execute及再次execute、风险保真、冲突拒绝和源库只读。");
            }
            finally
            {
                maintenance.Ado.ExecuteCommand($"DROP DATABASE IF EXISTS \"{sourceName}\" WITH (FORCE)");
                maintenance.Ado.ExecuteCommand($"DROP DATABASE IF EXISTS \"{targetName}\" WITH (FORCE)");
            }
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
