using GameFrameX.Entity.System;
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
                var types = new[] { typeof(TenantGameHomeOnlineSampleEntity) };
                target.CodeFirst.InitTables<SystemTenantEntity>();
                target.Insertable(new SystemTenantEntity { Id = 1300000000001, IsDeleted = false }).ExecuteCommand();
                source.CodeFirst.InitTables<TenantGameHomeOnlineSampleEntity>();
                source.Insertable(new TenantGameHomeOnlineSampleEntity { Id = 7001, TenantId = 1300000000001, SampleTime = 1000, OnlineCount = 42 }).ExecuteCommand();
                SingleMainMigration.Run(source, target, true, types);
                Require(!target.DbMaintenance.IsAnyTable("tenant_game_home_online_sample_entity", false), "dry-run 建表必须回滚");
                SingleMainMigration.Run(source, target, false, types);
                SingleMainMigration.Run(source, target, false, types);
                Require(target.Queryable<TenantGameHomeOnlineSampleEntity>().Count() == 1, "重复迁移必须幂等");
                Require(target.Queryable<TenantGameHomeOnlineSampleEntity>().Single().OnlineCount == 42, "业务字段必须保真");
                target.Updateable<TenantGameHomeOnlineSampleEntity>().SetColumns(row => row.OnlineCount == 99).Where(row => row.Id == 7001).ExecuteCommand();
                Reject(() => SingleMainMigration.Run(source, target, false, types), "冲突必须拒绝");
                Require(target.Queryable<TenantGameHomeOnlineSampleEntity>().Single().OnlineCount == 99, "冲突不得覆盖目标");
                target.Insertable(new SystemTenantEntity { Id = 9000, IsDeleted = false }).ExecuteCommand();
                Reject(() => SingleMainMigration.Run(source, target, true, types), "多租户必须拒绝");
                Require(source.Queryable<TenantGameHomeOnlineSampleEntity>().Single().OnlineCount == 42, "源库必须不变");
            }

            Console.WriteLine("迁移自测通过：dry-run 回滚、保真复制、幂等重跑、冲突拒绝、多租户拒绝、源库只读。");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    private static void Reject(Action operation, string message)
    {
        try
        {
            operation();
        }
        catch (InvalidOperationException)
        {
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
