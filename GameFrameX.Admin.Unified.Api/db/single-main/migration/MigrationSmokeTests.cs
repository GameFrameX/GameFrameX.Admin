using GameFrameX.Entity.System;
using GameFrameX.Entity.Tenant.ClientApiConfig;
using GameFrameX.Entity.Log;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;
using SqlSugar;

namespace GameFrameX.DatabaseMigration;

internal static class MigrationSmokeTests
{
    internal static void Run()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gfx-single-main-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            using (var source = SingleMainMigration.Connect(DbType.Sqlite, $"DataSource={Path.Combine(directory, "source.db")}"))
            using (var target = SingleMainMigration.Connect(DbType.Sqlite, $"DataSource={Path.Combine(directory, "target.db")}"))
            {
                var types = new[] { typeof(TenantGameHomeOnlineSampleEntity), typeof(TenantGameGoodsEntity), typeof(SystemUserEntity), typeof(SystemLogVisitEntity), typeof(TenantClientApiPolicyEntity) };
                target.Ado.ExecuteCommand("CREATE TABLE system_tenant_entity (id BIGINT PRIMARY KEY, is_deleted BOOLEAN)");
                target.Ado.ExecuteCommand("INSERT INTO system_tenant_entity VALUES (1300000000001, 0)");
                target.CodeFirst.InitTables<SystemUserEntity>();
                target.Ado.ExecuteCommand("ALTER TABLE system_user_entity ADD COLUMN tenant_id BIGINT NOT NULL DEFAULT 0");
                target.CodeFirst.InitTables<SystemLogVisitEntity>();
                target.CodeFirst.InitTables<GlobalMenuEntity>();
                target.Insertable(new GlobalMenuEntity { Id = 9001, Key = "permission:auto:migration", Title = "迁移菜单", AppScope = (MenuAppScopeEnum)2 }).ExecuteCommand();
                source.CodeFirst.InitTables<TenantGameGoodsEntity>();
                source.Insertable(new TenantGameGoodsEntity { Id = 8001, Name = "主库资源", ConfigId = 42 }).ExecuteCommand();
                source.CodeFirst.InitTables<TenantGameHomeOnlineSampleEntity>();
                source.Insertable(new TenantGameHomeOnlineSampleEntity { Id = 7001, SampleTime = 1000, OnlineCount = 42 }).ExecuteCommand();
                source.Ado.ExecuteCommand("ALTER TABLE tenant_game_home_online_sample_entity ADD COLUMN tenant_id BIGINT NOT NULL DEFAULT 1300000000001");
                source.Ado.ExecuteCommand("ALTER TABLE tenant_game_home_online_sample_entity ADD COLUMN app_id BIGINT NOT NULL DEFAULT 100");
                target.CodeFirst.InitTables<TenantGameHomeOnlineSampleEntity>();
                target.Ado.ExecuteCommand("DROP INDEX ux_home_online_sample_tenant_time");
                target.Ado.ExecuteCommand("ALTER TABLE tenant_game_home_online_sample_entity ADD COLUMN tenant_id BIGINT NOT NULL DEFAULT 1300000000001");
                target.Ado.ExecuteCommand("ALTER TABLE tenant_game_home_online_sample_entity ADD COLUMN app_id BIGINT NOT NULL DEFAULT 100");
                target.Ado.ExecuteCommand("CREATE UNIQUE INDEX ux_home_online_sample_tenant_time ON tenant_game_home_online_sample_entity (tenant_id, sample_time)");
                source.CodeFirst.InitTables<TenantClientApiPolicyEntity>();
                source.Insertable(new[] { new TenantClientApiPolicyEntity { Id = 9101 }, new TenantClientApiPolicyEntity { Id = 9102 } }).ExecuteCommand();
                source.Ado.ExecuteCommand("ALTER TABLE tenant_client_api_policy_entity ADD COLUMN app_id BIGINT NOT NULL DEFAULT 0");
                source.Ado.ExecuteCommand("UPDATE tenant_client_api_policy_entity SET app_id = 100 WHERE id = 9102");
                Reject(() => SingleMainMigration.Run(source, target, true, types), "默认层与应用层策略碰撞必须拒绝", "配置作用域碰撞");
                source.Deleteable<TenantClientApiPolicyEntity>().Where(row => row.Id == 9102).ExecuteCommand();
                SingleMainMigration.Run(source, target, true, types);
                Require(target.DbMaintenance.GetColumnInfosByTableName("tenant_game_home_online_sample_entity", false).Any(column => column.DbColumnName == "tenant_id"), "dry-run 删列必须回滚");
                Require(target.Queryable<TenantGameHomeOnlineSampleEntity>().Count() == 0, "dry-run 数据复制必须回滚");
                Require((int)target.Queryable<GlobalMenuEntity>().Single().AppScope == 2, "dry-run 菜单作用域更新必须回滚");
                Require(!target.DbMaintenance.IsAnyTable("tenant_game_goods_entity", false), "dry-run 新建表必须回滚");
                Require(target.DbMaintenance.GetColumnInfosByTableName("system_user_entity", false).Any(column => column.DbColumnName == "tenant_id"), "dry-run 系统表删列必须回滚");
                SingleMainMigration.Run(source, target, false, types);
                SingleMainMigration.Run(source, target, false, types);

                LogCredentialCleanupSmokeTests.Run(source, target);
                Require(target.Queryable<TenantGameHomeOnlineSampleEntity>().Count() == 1, "重复迁移必须幂等");
                Require(!target.DbMaintenance.GetColumnInfosByTableName("tenant_game_home_online_sample_entity", false).Any(column => column.DbColumnName is "tenant_id" or "app_id"), "业务作用域列必须删除");
                Require(!target.DbMaintenance.GetColumnInfosByTableName("system_user_entity", false).Any(column => column.DbColumnName == "tenant_id"), "系统表作用域列必须删除");
                Require(target.DbMaintenance.GetColumnInfosByTableName("system_log_visit_entity", false).Any(column => column.DbColumnName == "tenant_id"), "历史审计归属列必须保留");
                Require(target.Queryable<TenantGameGoodsEntity>().Single().ConfigId == 42, "新建业务表应保真复制");
                Require(target.Queryable<GlobalMenuEntity>().Single().AppScope == MenuAppScopeEnum.Console, "手建菜单作用域必须塌缩");
                Reject(() => target.Insertable(new TenantGameHomeOnlineSampleEntity { Id = 7002, SampleTime = 1000, OnlineCount = 1 }).ExecuteCommand(), "业务时间键仍应唯一", "UNIQUE");
                Require(target.Queryable<TenantGameHomeOnlineSampleEntity>().Single().OnlineCount == 42, "业务字段必须保真");
                target.Insertable(new TenantClientApiPolicyEntity { Id = 9103 }).ExecuteCommand();
                Reject(() => SingleMainMigration.Run(source, target, false, types), "源目标不同主键的策略碰撞必须拒绝", "配置作用域碰撞");
                Require(target.Queryable<TenantClientApiPolicyEntity>().Count() == 2, "拒绝迁移不能改写原目标配置");
                target.Deleteable<TenantClientApiPolicyEntity>().Where(row => row.Id == 9103).ExecuteCommand();
                target.Updateable<TenantGameHomeOnlineSampleEntity>().SetColumns(row => row.OnlineCount == 99).Where(row => row.Id == 7001).ExecuteCommand();
                Reject(() => SingleMainMigration.Run(source, target, false, types), "冲突必须拒绝", "主键碰撞");
                Require(target.Queryable<TenantGameHomeOnlineSampleEntity>().Single().OnlineCount == 99, "冲突不得覆盖目标");
                target.Ado.ExecuteCommand("INSERT INTO system_tenant_entity VALUES (9000, 0)");
                Reject(() => SingleMainMigration.Run(source, target, true, types), "多租户必须拒绝", "多租户");
                Require(source.Queryable<TenantGameHomeOnlineSampleEntity>().Single().OnlineCount == 42, "源库必须不变");
            }

            LegacyRiskPolicySmokeTests.Run(directory);
            Console.WriteLine("迁移自测通过：dry-run 删列/复制回滚、业务索引重建、保真复制、幂等重跑、冲突拒绝、多租户拒绝、源库只读。");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    private static void Reject(Action operation, string message, string expectedError)
    {
        try
        {
            operation();
        }
        catch (Exception error)
        {
            Require(error.Message.Contains(expectedError, StringComparison.OrdinalIgnoreCase), message + "，实际异常：" + error.Message);
            return;
        }

        throw new InvalidOperationException(message);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
