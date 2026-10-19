using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Entity.Tenant;

/// <summary>
/// 系统职位表
/// </summary>
[SugarTable(null, "租户库职位表")]
[TenantTable]
[IncrementTable]
public class TenantPositionEntity : EntityTenantBase
{
    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = EntityStringLength.Length256)]
    public virtual string Name { get; set; } = null!;

    /// <summary>
    /// 编码
    /// </summary>
    [SugarColumn(ColumnDescription = "编码", Length = EntityStringLength.Length256)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序", DefaultValue = "0")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 备注
    /// </summary>
    /// <remarks>
    /// 默认空串而非 null：列为 NOT NULL，新建职位表单备注非必填（未填时 JSON 省略字段，
    /// 保留本默认值），Add 落库不再违反非空约束（GFX-918 / C387）。
    /// </remarks>
    [SugarColumn(ColumnDescription = "备注", Length = EntityStringLength.Length256)]
    public string Remark { get; set; } = string.Empty;
}