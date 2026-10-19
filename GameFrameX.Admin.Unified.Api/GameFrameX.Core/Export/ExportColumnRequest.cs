namespace GameFrameX.Core.Export;

/// <summary>
/// 导出列定义。Field 为 TResponse 上的属性名，Header 为 Excel 列标题。
/// </summary>
public class ExportColumnRequest
{
    /// <summary>导出字段名，须与实体 Output DTO 的属性名匹配（忽略大小写）。</summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>Excel 列标题；为空时回退为属性名。</summary>
    public string Header { get; set; } = string.Empty;
}