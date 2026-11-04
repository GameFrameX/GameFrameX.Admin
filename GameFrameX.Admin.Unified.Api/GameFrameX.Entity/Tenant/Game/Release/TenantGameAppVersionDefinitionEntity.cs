using GameFrameX.Entity.Base;
using GameFrameX.Foundation.Orm.Entity.Filter;

namespace GameFrameX.Entity.Tenant.Game.Release;

/// <summary>
/// 客户端程序版本号字典（应用作用域事实数据）。
/// </summary>
[SugarTable(null, "客户端程序版本配置表")]
[SugarIndex("idx_version_definition_name", nameof(Name), OrderByType.Asc, true)]
[TenantTable]
[IncrementTable]
public sealed class TenantGameAppVersionDefinitionEntity : EntityBasic, ISelectFilter
{

    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = EntityStringLength.Length256)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    [SugarColumn(ColumnDescription = "详细描述", Length = EntityStringLength.Length1024)]
    public string Description { get; set; } = null!;
}