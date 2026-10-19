using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Entity.Tenant;

/// <summary>
/// 系统打印模板表
/// </summary>
[SugarTable(null, "租户库打印模板表")]
[TenantTable]
[IncrementTable]
public class TenantPrintEntity : EntityTenantBase
{
    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = EntityStringLength.Length256)]
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>
    /// 打印模板
    /// </summary>
    [SugarColumn(ColumnDescription = "打印模板", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public virtual string Template { get; set; } = string.Empty;

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序", DefaultValue = "0")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = EntityStringLength.Length256)]
    public string? Remark { get; set; }
}