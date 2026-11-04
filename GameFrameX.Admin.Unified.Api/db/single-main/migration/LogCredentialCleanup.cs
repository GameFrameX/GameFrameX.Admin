using System.Data;
using GameFrameX.Core.Logging;
using SqlSugar;

namespace GameFrameX.DatabaseMigration;

/// <summary>仅处理日志白名单；使用与运行时相同的脱敏规则，不输出日志内容。</summary>
internal static class LogCredentialCleanup
{
    internal static bool IsLogTable(string tableName)
    {
        return new[] { "system", "tenant" }.Any(prefix => new[] { "operation", "exception", "visit", "diff", "audit" }
            .Any(kind => tableName.Equals($"{prefix}_log_{kind}_entity", StringComparison.OrdinalIgnoreCase)));
    }

    internal static void RedactRow(string tableName, DataRow row)
    {
        if (!IsLogTable(tableName))
        {
            return;
        }

        var snapshot = new System.Text.Json.Nodes.JsonObject();
        foreach (DataColumn column in row.Table.Columns)
        {
            if (row[column] is string value)
            {
                snapshot[column.ColumnName] = value;
            }
        }

        var redacted = System.Text.Json.Nodes.JsonNode.Parse(SensitiveLogRedactor.Redact(snapshot.ToJsonString())!)!;
        foreach (DataColumn column in row.Table.Columns)
        {
            if (row[column] is string value)
            {
                row[column] = tableName.EndsWith("_diff_entity", StringComparison.OrdinalIgnoreCase)
                    && column.ColumnName.ToLowerInvariant() is "sql" or "parameters"
                    ? SensitiveLogRedactor.Mask : redacted[column.ColumnName]!.GetValue<string>();
            }
        }
    }

    internal static int Apply(ISqlSugarClient database, bool dryRun)
    {
        var affected = 0;
        foreach (var table in database.DbMaintenance.GetTableInfoList(false).Where(table => IsLogTable(table.Name)))
        {
            var rows = database.Ado.GetDataTable($"SELECT * FROM {Quote(table.Name)}");
            var changedRows = 0;
            foreach (DataRow row in rows.Rows)
            {
                var original = row.ItemArray;
                RedactRow(table.Name, row);
                var columns = rows.Columns.Cast<DataColumn>().Where(column => !Equals(row[column], original[column.Ordinal])).ToArray();
                if (columns.Length == 0)
                {
                    continue;
                }

                changedRows++;
                if (!dryRun)
                {
                    var parameters = columns.Select((column, index) => new SugarParameter($"@value{index}", row[column], column.DataType)).ToList();
                    parameters.Add(new SugarParameter("@identity", row["id"]));
                    database.Ado.ExecuteCommand($"UPDATE {Quote(table.Name)} SET {string.Join(", ", columns.Select((column, index) => $"{Quote(column.ColumnName)} = @value{index}"))} WHERE id = @identity", parameters);
                }
            }

            affected += changedRows;
            Console.WriteLine($"日志脱敏 {table.Name}: 受影响 {changedRows} 行{(dryRun ? "（仅预览）" : "")}。");
        }

        return affected;
    }

    private static string Quote(string identifier)
    {
        return "\"" + identifier.Replace("\"", "\"\"") + "\"";
    }
}
