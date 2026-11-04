using GameFrameX.Core.Logging;
using GameFrameX.Entity.Log;
using SqlSugar;

namespace GameFrameX.DatabaseMigration;

internal static class LogCredentialCleanupSmokeTests
{
    internal static void Run(ISqlSugarClient source, ISqlSugarClient target)
    {
        const string plain = "historical-login-credential-23456";
        var payload = "{\"Password\":\"" + plain + "\",\"AccessToken\":\"" + plain + "\",\"RefreshToken\":\"" + plain + "\"}";
        source.CodeFirst.InitTables<TenantLogOperationEntity, TenantLogAuditEntity>();
        target.CodeFirst.InitTables<SystemLogOperationEntity, SystemLogAuditEntity>();
        var row = new TenantLogOperationEntity { Id = 6401 };
        foreach (var property in typeof(TenantLogOperationEntity).GetProperties().Where(property => property.CanWrite && property.PropertyType == typeof(string)))
        {
            property.SetValue(row, string.Empty);
        }

        row.Message = payload;
        row.RequestParam = payload;
        row.ReturnResult = payload;
        row.Exception = plain;
        source.Insertable(row).ExecuteCommand();
        var mainRow = new SystemLogOperationEntity { Id = 6402 };
        foreach (var property in typeof(SystemLogOperationEntity).GetProperties().Where(property => property.CanWrite && property.PropertyType == typeof(string)))
        {
            property.SetValue(mainRow, string.Empty);
        }

        mainRow.Message = payload;
        mainRow.Exception = plain;
        target.Insertable(mainRow).ExecuteCommand();
        source.Insertable(new TenantLogAuditEntity { Id = 6410, TableName = "system_user_entity", ColumnName = "Password", OldValue = plain, NewValue = plain }).ExecuteCommand();
        target.Insertable(new SystemLogAuditEntity { Id = 6411, TableName = "system_user_entity", ColumnName = "Password", OldValue = plain, NewValue = plain }).ExecuteCommand();
        Require(LogCredentialCleanup.Apply(target, true) == 2, "独立清理预览仅报告受影响行数");
        Require(target.Queryable<SystemLogOperationEntity>().InSingle(6402).Message == payload, "清理预览不得改写数据");
        var types = new[] { typeof(TenantLogOperationEntity), typeof(SystemLogOperationEntity), typeof(TenantLogAuditEntity), typeof(SystemLogAuditEntity) };
        SingleMainMigration.Run(source, target, true, types);
        Require(target.Queryable<SystemLogOperationEntity>().InSingle(6402).Message == payload, "迁移预览必须回滚脱敏");
        SingleMainMigration.Run(source, target, false, types);
        SingleMainMigration.Run(source, target, false, types);
        Require(!target.Queryable<SystemLogOperationEntity>().InSingle(6402).Message!.Contains(plain) && !target.Queryable<SystemLogOperationEntity>().InSingle(6402).Exception!.Contains(plain), "主库旧日志必须脱敏");
        var migrated = target.Queryable<TenantLogOperationEntity>().InSingle(6401);
        Require(!migrated.Message!.Contains(plain) && !migrated.RequestParam!.Contains(plain) && !migrated.ReturnResult!.Contains(plain) && !migrated.Exception!.Contains(plain), "迁入日志各快照必须脱敏");
        Require(target.Queryable<SystemLogAuditEntity>().InSingle(6411).OldValue == SensitiveLogRedactor.Mask && target.Queryable<SystemLogAuditEntity>().InSingle(6411).NewValue == SensitiveLogRedactor.Mask, "主库审计新旧值必须脱敏");
        Require(target.Queryable<TenantLogAuditEntity>().InSingle(6410).OldValue == SensitiveLogRedactor.Mask && target.Queryable<TenantLogAuditEntity>().InSingle(6410).NewValue == SensitiveLogRedactor.Mask, "迁入审计新旧值必须脱敏");
        Require(source.Queryable<TenantLogAuditEntity>().InSingle(6410).OldValue == plain, "源审计值不得改写");
        Require(source.Queryable<TenantLogOperationEntity>().InSingle(6401).Message == payload, "源日志不得改写");
        Require(LogCredentialCleanup.Apply(target, false) == 0, "再次清理必须幂等");
        // 模拟尚未运行整库迁移的主库，独立清理模式同样可以收敛。
        target.Updateable<SystemLogOperationEntity>().SetColumns(item => item.Message == payload).Where(item => item.Id == 6402).ExecuteCommand();
        Require(LogCredentialCleanup.Apply(target, false) == 1 && LogCredentialCleanup.Apply(target, false) == 0, "独立清理必须幂等");
        Console.WriteLine("日志凭据自测通过：主库清理、迁入脱敏、预览不写、源只读、execute 重跑与独立清理幂等。");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
