using GameFrameX.Entity.Tenant.ClientApiConfig;
using SqlSugar;

namespace GameFrameX.DatabaseMigration;

internal static class LegacyRiskPolicySmokeTests
{
    internal static void Run(string directory)
    {
        Verify(directory, "existing", true, "AuditOnly");
        Verify(directory, "existing-target", true, "FailClosed", true);
        Verify(directory, "missing", false, null);
        Console.WriteLine("风险迁移自测通过：旧策略保真、缺策略补建、空值保留、dry-run 回滚、幂等、源目标风险冲突与禁用策略拒绝。");
    }

    private static void Verify(string directory, string name, bool existingPolicy, string? riskPolicy, bool existingTarget = false)
    {
        using (var source = SingleMainMigration.Connect(DbType.Sqlite, $"DataSource={Path.Combine(directory, name + "-source.db")}"))
        using (var target = SingleMainMigration.Connect(DbType.Sqlite, $"DataSource={Path.Combine(directory, name + "-target.db")}"))
        {
            var types = new[] { typeof(TenantClientApiPolicyEntity) };
            target.Ado.ExecuteCommand("CREATE TABLE system_tenant_entity (id BIGINT PRIMARY KEY, is_deleted BOOLEAN)");
            target.Ado.ExecuteCommand("INSERT INTO system_tenant_entity VALUES (1300000000001, 0)");
            source.Ado.ExecuteCommand("CREATE TABLE tenant_game_app_entity (id BIGINT PRIMARY KEY, is_deleted BOOLEAN, signature_required BOOLEAN, device_proof_required BOOLEAN, risk_policy TEXT)");
            source.Ado.ExecuteCommand("INSERT INTO tenant_game_app_entity VALUES (8101, false, true, true, @risk)", new SugarParameter("@risk", riskPolicy));
            if (existingPolicy)
            {
                source.CodeFirst.InitTables<TenantClientApiPolicyEntity>();
                source.Insertable(new TenantClientApiPolicyEntity { Id = 9101, IsEnabled = true, SigningKey = "existing-signing-key", LifetimeMinutes = 37 }).ExecuteCommand();
                if (existingTarget)
                {
                    target.CodeFirst.InitTables<TenantClientApiPolicyEntity>();
                    target.Insertable(new TenantClientApiPolicyEntity { Id = 9101, IsEnabled = true, SigningKey = "existing-signing-key", LifetimeMinutes = 37 }).ExecuteCommand();
                }

                foreach (var column in new[] { "signature_required", "device_proof_required", "risk_policy" })
                {
                    source.Ado.ExecuteCommand($"ALTER TABLE tenant_client_api_policy_entity DROP COLUMN {column}");
                    if (existingTarget)
                    {
                        target.Ado.ExecuteCommand($"ALTER TABLE tenant_client_api_policy_entity DROP COLUMN {column}");
                    }
                }
            }

            SingleMainMigration.Run(source, target, true, types);
            Require(existingTarget
                ? !target.DbMaintenance.GetColumnInfosByTableName("tenant_client_api_policy_entity", false).Any(column => column.DbColumnName == "risk_policy")
                : !target.DbMaintenance.IsAnyTable("tenant_client_api_policy_entity", false), "dry-run 风险建表和更新必须回滚");
            SingleMainMigration.Run(source, target, false, types);
            SingleMainMigration.Run(source, target, false, types);
            var policy = target.Queryable<TenantClientApiPolicyEntity>().Single();
            Require(policy.SignatureRequired && policy.DeviceProofRequired && (riskPolicy == null ? target.Ado.GetInt("SELECT COUNT(*) FROM tenant_client_api_policy_entity WHERE risk_policy IS NULL") == 1 : policy.RiskPolicy == riskPolicy) && policy.IsEnabled == true, "风险三字段及空值必须原样迁移");
            Require(policy.Id == (existingPolicy ? 9101 : 8101), "风险策略身份必须稳定");
            if (existingPolicy)
            {
                Require(policy.SigningKey == "existing-signing-key" && policy.LifetimeMinutes == 37, "原策略其他字段不得改写");
            }

            target.Updateable<TenantClientApiPolicyEntity>().SetColumns(row => row.SignatureRequired == false).Where(row => row.Id == policy.Id).ExecuteCommand();
            Reject(() => SingleMainMigration.Run(source, target, false, types), "风险配置冲突");
            Require(!target.Queryable<TenantClientApiPolicyEntity>().Single().SignatureRequired, "冲突不得覆盖目标设置");
            target.Updateable<TenantClientApiPolicyEntity>().SetColumns(row => row.SignatureRequired == true).Where(row => row.Id == policy.Id).ExecuteCommand();
            target.Updateable<TenantClientApiPolicyEntity>().SetColumns(row => row.IsEnabled == false).Where(row => row.Id == policy.Id).ExecuteCommand();
            Reject(() => SingleMainMigration.Run(source, target, false, types), existingPolicy ? "主键碰撞" : "唯一启用策略");
            Require(target.Queryable<TenantClientApiPolicyEntity>().Single().IsEnabled == false, "禁用策略不得被自动启用");
            Require(source.Ado.GetInt("SELECT COUNT(*) FROM tenant_game_app_entity WHERE signature_required = true AND device_proof_required = true") == 1, "源应用风险配置必须保持不变");

            if (!existingPolicy)
            {
                source.CodeFirst.InitTables<TenantClientApiPolicyEntity>();
                source.Insertable(new TenantClientApiPolicyEntity { Id = 9201, IsEnabled = true, SignatureRequired = false }).ExecuteCommand();
                Reject(() => SingleMainMigration.Run(source, target, true, types), "风险配置冲突");
            }
        }
    }

    private static void Reject(Action operation, string message)
    {
        try
        {
            operation();
        }
        catch (InvalidOperationException exception)
        {
            Require(exception.Message.Contains(message, StringComparison.Ordinal), "实际异常：" + exception.Message);
            return;
        }

        throw new InvalidOperationException("应拒绝迁移：" + message);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
