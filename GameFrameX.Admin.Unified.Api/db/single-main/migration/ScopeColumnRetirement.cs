using System.Text.RegularExpressions;
using SqlSugar;

namespace GameFrameX.DatabaseMigration;

internal static class ScopeColumnRetirement
{
    private static readonly string[] RetiredColumns = { "tenant_id", "app_id", "game_app_id", "game_app_id_normalized" };

    internal static void Apply(ISqlSugarClient target, IEnumerable<Type> entityTypes)
    {
        var existingTables = target.DbMaintenance.GetTableInfoList(false).Select(table => table.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var entityType in entityTypes)
        {
            var entity = target.EntityMaintenance.GetEntityInfo(entityType);
            if (!existingTables.Contains(entity.DbTableName))
            {
                continue;
            }

            // 当前模型仍声明的同名字段（历史审计归属、支付品牌标识）不能删除。
            var currentColumns = entity.Columns.Where(column => !column.IsIgnore).Select(column => column.DbColumnName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var removedColumns = target.DbMaintenance.GetColumnInfosByTableName(entity.DbTableName, false)
                .Select(column => column.DbColumnName)
                .Where(column => RetiredColumns.Contains(column, StringComparer.OrdinalIgnoreCase) && !currentColumns.Contains(column))
                .ToArray();
            if (removedColumns.Length == 0)
            {
                continue;
            }

            if (target.CurrentConnectionConfig.DbType == DbType.Sqlite)
            {
                var indexes = target.Ado.GetDataTable("SELECT name, sql FROM sqlite_master WHERE type = 'index' AND tbl_name = @table AND sql IS NOT NULL",
                    new SugarParameter("@table", entity.DbTableName));
                foreach (System.Data.DataRow index in indexes.Rows)
                {
                    var definition = Convert.ToString(index["sql"]) ?? string.Empty;
                    if (removedColumns.Any(column => Regex.IsMatch(definition, @"\b" + Regex.Escape(column) + @"\b", RegexOptions.IgnoreCase)))
                    {
                        target.Ado.ExecuteCommand($"DROP INDEX {Quote(Convert.ToString(index["name"])!)}");
                    }
                }
            }

            foreach (var column in removedColumns)
            {
                // PostgreSQL 同时移除依赖该列的旧索引；不使用 CASCADE 删除其他对象。
                target.Ado.ExecuteCommand($"ALTER TABLE {Quote(entity.DbTableName)} DROP COLUMN {Quote(column)}");
                Console.WriteLine($"删除退场列：{entity.DbTableName}.{column}");
            }

            target.Utilities.RemoveCacheAll();
            // 按当前实体的业务键重建索引；塌缩后的唯一键冲突会终止并回滚迁移。
            target.CodeFirst.InitTables(entityType);
        }
    }

    private static string Quote(string identifier)
    {
        return "\"" + identifier.Replace("\"", "\"\"") + "\"";
    }
}
