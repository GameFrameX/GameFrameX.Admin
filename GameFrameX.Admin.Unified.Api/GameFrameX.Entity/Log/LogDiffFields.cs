using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Log;

/// <summary>
/// 差异日志公共字段基类。不映射物理表，供 <see cref="SystemLogDiffEntity"/> 与 <see cref="TenantLogDiffEntity"/> 复用字段。
/// </summary>
public class LogDiffFields : EntityBasic
{
    /// <summary>
    /// 操作前记录
    /// </summary>
    [SugarColumn(ColumnDescription = "操作前记录", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? BeforeData { get; set; }

    /// <summary>
    /// 操作后记录
    /// </summary>
    [SugarColumn(ColumnDescription = "操作后记录", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? AfterData { get; set; }

    /// <summary>
    /// 全字段快照（操作后的完整字段，事后排查用）
    /// </summary>
    [SugarColumn(ColumnDescription = "全字段快照", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Snapshot { get; set; }

    /// <summary>
    /// Sql
    /// </summary>
    [SugarColumn(ColumnDescription = "Sql", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Sql { get; set; }

    /// <summary>
    /// 参数  手动传入的参数
    /// </summary>
    [SugarColumn(ColumnDescription = "参数", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Parameters { get; set; }

    /// <summary>
    /// 业务对象
    /// </summary>
    [SugarColumn(ColumnDescription = "业务对象", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? BusinessData { get; set; }

    /// <summary>
    /// 差异操作
    /// </summary>
    [SugarColumn(ColumnDescription = "差异操作", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? DiffType { get; set; }

    /// <summary>
    /// 耗时
    /// </summary>
    [SugarColumn(ColumnDescription = "耗时")]
    public long? Elapsed { get; set; }
}