using MiniExcelLibs;

namespace GameFrameX.Core.Export;

/// <summary>
/// 通用 Excel 导出工具。按列白名单投影数据行生成 .xlsx 文件结果。
/// </summary>
/// <remarks>
/// 安全规则：
/// 1) 仅输出 <see cref="BuildExcelResultAsync{T}"/> columns 白名单中、且 T 真实存在对应属性的字段；
/// 2) 标记 <see cref="ExportSensitiveAttribute"/> 的属性即使在白名单中也强制剔除；
/// 3) 不解析任意 SQL / 表名，仅对已查询入参的内存对象做属性投影。
/// </remarks>
public static class DataExportHelper
{
    /// <summary>
    /// 单次同步全量导出的最大行数上限，避免大数据量阻塞请求；
    /// 超量需求应通过衍生 change 引入异步导出队列 + 下载中心。
    /// </summary>
    public const int MaxExportRows = 10000;

    /// <summary>
    /// 按列白名单投影数据行并生成 Excel 文件结果。
    /// </summary>
    /// <typeparam name="T">行数据类型（通常为实体 Output DTO）。</typeparam>
    /// <param name="rows">已查询并通过租户/权限过滤的数据行。</param>
    /// <param name="columns">前端可见列白名单。</param>
    /// <param name="fileSuffix">文件名后缀（业务名，如「租户渠道定义」）。</param>
    /// <returns>可直接返回的 .xlsx FileStreamResult。</returns>
    public static async Task<IActionResult> BuildExcelResultAsync<T>(
        IEnumerable<T> rows,
        IReadOnlyList<ExportColumnRequest> columns,
        string fileSuffix)
    {
        var resolvedColumns = ResolveColumns<T>(columns);
        var dictionaryRows = ProjectRows(rows, resolvedColumns);

        var stream = new MemoryStream();
        await stream.SaveAsAsync(dictionaryRows, excelType: ExcelType.XLSX);
        stream.Position = 0;

        var fileName = DateTime.Now.ToString("yyyyMMddHHmm") + fileSuffix + ".xlsx";
        return new FileStreamResult(stream, "application/octet-stream")
        {
            FileDownloadName = fileName,
        };
    }

    /// <summary>
    /// 解析有效导出列：属性必须存在且未标记敏感。
    /// </summary>
    private static List<ResolvedExportColumn> ResolveColumns<T>(IReadOnlyList<ExportColumnRequest> columns)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var byName = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in properties)
        {
            byName[property.Name] = property;
        }

        var resolved = new List<ResolvedExportColumn>();
        if (columns == null)
        {
            return resolved;
        }

        var seenHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var column in columns)
        {
            if (string.IsNullOrWhiteSpace(column?.Field))
            {
                continue;
            }

            if (!byName.TryGetValue(column.Field, out var property))
            {
                // 非白名单或属性不存在：不输出。
                continue;
            }

            if (property.GetCustomAttribute<ExportSensitiveAttribute>() != null)
            {
                // 敏感字段：后端强制剔除，即便前端请求。
                continue;
            }

            var header = string.IsNullOrWhiteSpace(column.Header) ? property.Name : column.Header;
            if (!seenHeaders.Add(header))
            {
                // 列标题重复时跳过，避免 MiniExcel 字典列冲突。
                continue;
            }

            resolved.Add(new ResolvedExportColumn(property, header));
        }

        return resolved;
    }

    /// <summary>
    /// 将行集合投影为字典集合，按列白名单取值。
    /// </summary>
    private static List<Dictionary<string, object?>> ProjectRows<T>(IEnumerable<T> rows, List<ResolvedExportColumn> columns)
    {
        var list = new List<Dictionary<string, object?>>();
        if (columns.Count == 0)
        {
            return list;
        }

        var source = rows ?? Enumerable.Empty<T>();
        foreach (var row in source)
        {
            var dictionary = new Dictionary<string, object?>(columns.Count, StringComparer.Ordinal);
            foreach (var column in columns)
            {
                dictionary[column.Header] = NormalizeValue(column.Property.GetValue(row));
            }

            list.Add(dictionary);
        }

        return list;
    }

    /// <summary>
    /// 规范化单元格值：枚举导出为名称字符串，其它原样输出。
    /// </summary>
    private static object? NormalizeValue(object? value)
    {
        if (value == null)
        {
            return null;
        }

        var type = value.GetType();
        if (type.IsEnum)
        {
            return value.ToString();
        }

        return value;
    }

    private sealed class ResolvedExportColumn
    {
        public ResolvedExportColumn(PropertyInfo property, string header)
        {
            Property = property;
            Header = header;
        }

        public PropertyInfo Property { get; }

        public string Header { get; }
    }
}