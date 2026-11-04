using System.Data;
using System.Globalization;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.Tenant;
using SqlSugar;
using DatabaseType = SqlSugar.DbType;

namespace GameFrameX.DatabaseMigration;

internal static class SingleMainMigration
{
    private const long DefaultTenantId = 1300000000001;

    internal static SqlSugarClient Connect(DatabaseType databaseType, string connectionString)
    {
        if (databaseType != DatabaseType.Sqlite && databaseType != DatabaseType.PostgreSQL)
        {
            throw new InvalidOperationException("迁移仅支持 SQLite dry-run 与 PostgreSQL 发布库。");
        }

        var configuration = new ConnectionConfig
        {
            ConfigId = "main",
            DbType = databaseType,
            ConnectionString = connectionString,
            IsAutoCloseConnection = false,
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(configuration);
        configuration.IsAutoCloseConnection = false;
        return new SqlSugarClient(configuration);
    }

    internal static void Run(ISqlSugarClient source, ISqlSugarClient target, bool dryRun, Type[]? entityTypes = null)
    {
        if (source.CurrentConnectionConfig.ConnectionString == target.CurrentConnectionConfig.ConnectionString)
        {
            throw new InvalidOperationException("源库与目标库必须不同。");
        }

        if (source.CurrentConnectionConfig.DbType != target.CurrentConnectionConfig.DbType)
        {
            throw new InvalidOperationException("源库与目标库必须使用相同数据库类型。");
        }

        source.Ado.BeginTran(IsolationLevel.Serializable);
        target.Ado.BeginTran(IsolationLevel.Serializable);
        try
        {
            source.Ado.ExecuteCommand(source.CurrentConnectionConfig.DbType == DatabaseType.Sqlite
                ? "PRAGMA query_only = ON" : "SET TRANSACTION READ ONLY");
            ValidateDefaultInstance(source, target);
            var riskMigration = LegacyRiskPolicyMigration.Prepare(source, target);
            ConfigurationCollisionGuard.Validate(source, true);
            ConfigurationCollisionGuard.Validate(target, true);
            var currentTypes = entityTypes ?? typeof(TenantUserEntity).Assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && type.IsDefined(typeof(SugarTable), false))
                .OrderBy(type => type.FullName, StringComparer.Ordinal).ToArray();
            ScopeColumnRetirement.Apply(target, currentTypes);
            riskMigration?.CompleteLegacyTarget(target);
            NormalizeMenuScope(target);
            var types = currentTypes.Where(SqlSugarEntityBoundary.IsTenantTable).ToArray();
            var sourceTables = source.DbMaintenance.GetTableInfoList(false).Select(table => table.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var copiedTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entityType in types)
            {
                var tableName = target.EntityMaintenance.GetEntityInfo(entityType).DbTableName;
                if (!sourceTables.Contains(tableName))
                {
                    continue;
                }

                // 目标业务表按当前实体创建；已退场实体不在程序集清单，不搬运。
                target.CodeFirst.InitTables(entityType);
                CopyTable(source, target, tableName, riskMigration);
                copiedTables.Add(tableName);
            }

            riskMigration?.Apply(target);
            ConfigurationCollisionGuard.Validate(target, false);

            foreach (var ignored in sourceTables.Except(copiedTables).OrderBy(name => name, StringComparer.Ordinal))
            {
                Console.WriteLine($"未迁移表：{ignored}");
            }

            if (dryRun)
            {
                target.Ado.RollbackTran();
                Console.WriteLine("dry-run 完成：已执行删列、业务索引重建、建表、碰撞核对、复制并回滚全部目标变更。");
            }
            else
            {
                target.Ado.CommitTran();
                Console.WriteLine("迁移已提交；重复执行只核对既有行，不覆盖目标值。");
            }
        }
        catch
        {
            target.Ado.RollbackTran();
            throw;
        }
        finally
        {
            source.Ado.RollbackTran();
            if (source.CurrentConnectionConfig.DbType == DatabaseType.Sqlite)
            {
                source.Ado.ExecuteCommand("PRAGMA query_only = OFF");
            }

            target.Utilities.RemoveCacheAll();
        }
    }

    private static void NormalizeMenuScope(ISqlSugarClient target)
    {
        var tableName = target.EntityMaintenance.GetEntityInfo<GlobalMenuEntity>().DbTableName;
        if (target.DbMaintenance.IsAnyTable(tableName, false)
            && target.DbMaintenance.GetColumnInfosByTableName(tableName, false).Any(column => column.DbColumnName == "app_scope"))
        {
            target.Ado.ExecuteCommand($"UPDATE {Quote(tableName)} SET app_scope = 0 WHERE app_scope <> 0");
        }
    }

    private static void ValidateDefaultInstance(ISqlSugarClient source, ISqlSugarClient target)
    {
        const string registryTable = "system_tenant_entity";
        if (!target.DbMaintenance.IsAnyTable(registryTable, false))
        {
            throw new InvalidOperationException("目标必须是已有主库，缺少租户登记表，无法确认默认实例。");
        }

        var registered = target.Ado.GetDataTable($"SELECT id FROM {Quote(registryTable)} WHERE is_deleted = false");
        if (registered.Rows.Count != 1 || Convert.ToInt64(registered.Rows[0][0], CultureInfo.InvariantCulture) != DefaultTenantId)
        {
            throw new InvalidOperationException("只支持主库仅登记默认租户的自托管实例；多租户老库必须另行迁移。");
        }

        if (source.DbMaintenance.IsAnyTable("tenant_game_app_entity", false))
        {
            var applications = source.Ado.GetDataTable("SELECT id FROM tenant_game_app_entity WHERE is_deleted = false");
            if (applications.Rows.Count > 1)
            {
                throw new InvalidOperationException("源库包含多个应用，无法自动塌缩配置。");
            }
        }
    }

    private static void CopyTable(ISqlSugarClient source, ISqlSugarClient target, string tableName, LegacyRiskPolicyMigration? riskMigration)
    {
        var sourceColumns = source.DbMaintenance.GetColumnInfosByTableName(tableName, false);
        var targetColumns = target.DbMaintenance.GetColumnInfosByTableName(tableName, false);
        var columns = targetColumns.Select(column => column.DbColumnName)
            .Where(column => sourceColumns.Any(sourceColumn => string.Equals(sourceColumn.DbColumnName, column, StringComparison.OrdinalIgnoreCase)))
            .ToArray();
        if (!columns.Any(column => string.Equals(column, "id", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"表 {tableName} 无 id 主键，拒绝自动合并。");
        }

        var scopeColumn = sourceColumns.FirstOrDefault(column => string.Equals(column.DbColumnName, "tenant_id", StringComparison.OrdinalIgnoreCase));
        if (scopeColumn != null && source.Ado.GetInt($"SELECT COUNT(*) FROM {Quote(tableName)} WHERE {Quote(scopeColumn.DbColumnName)} IS NOT NULL AND {Quote(scopeColumn.DbColumnName)} NOT IN (0, {DefaultTenantId})") > 0)
        {
            throw new InvalidOperationException($"表 {tableName} 含非默认租户记录。");
        }

        var sourceColumnList = string.Join(", ", columns.Select(Quote));
        var rows = source.Ado.GetDataTable($"SELECT {sourceColumnList} FROM {Quote(tableName)} ORDER BY id");
        riskMigration?.EnrichCopiedPolicy(tableName, rows);
        columns = rows.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();
        var columnList = string.Join(", ", columns.Select(Quote));
        var inserted = 0;
        foreach (DataRow row in rows.Rows)
        {
            var existing = target.Ado.GetDataTable($"SELECT {columnList} FROM {Quote(tableName)} WHERE id = @identity", new SugarParameter("@identity", row["id"]));
            if (existing.Rows.Count != 0)
            {
                if (columns.Any(column => !ValuesEqual(row[column], existing.Rows[0][column])))
                {
                    throw new InvalidOperationException($"表 {tableName} 主键碰撞且内容不同；未覆盖目标数据。");
                }

                continue;
            }

            var parameters = columns.Select((column, index) => new SugarParameter($"@value{index}", row[column], rows.Columns[column]!.DataType)).ToArray();
            target.Ado.ExecuteCommand($"INSERT INTO {Quote(tableName)} ({columnList}) VALUES ({string.Join(", ", parameters.Select(parameter => parameter.ParameterName))})", parameters);
            inserted++;
        }

        Console.WriteLine($"{tableName}: 检查 {rows.Rows.Count} 行，新增 {inserted} 行。");
    }

    private static bool ValuesEqual(object left, object right)
    {
        if (left is DBNull || right is DBNull)
        {
            return left is DBNull && right is DBNull;
        }

        if (left is bool || right is bool)
        {
            return Convert.ToBoolean(left, CultureInfo.InvariantCulture) == Convert.ToBoolean(right, CultureInfo.InvariantCulture);
        }

        if (left is byte[] leftBytes && right is byte[] rightBytes)
        {
            return leftBytes.SequenceEqual(rightBytes);
        }

        return Convert.ToString(left, CultureInfo.InvariantCulture) == Convert.ToString(right, CultureInfo.InvariantCulture);
    }

    private static string Quote(string identifier)
    {
        return "\"" + identifier.Replace("\"", "\"\"") + "\"";
    }
}
