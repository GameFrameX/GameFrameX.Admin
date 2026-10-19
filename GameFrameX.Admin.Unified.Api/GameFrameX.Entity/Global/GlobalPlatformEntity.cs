using GameFrameX.Entity.Base;
using GameFrameX.Entity.Routing;
using GameFrameX.Foundation.Orm.Entity.Filter;

namespace GameFrameX.Entity.Global;

[SugarTable(null, "全局平台配置表")]
[GlobalTable]
[IncrementTable]
public sealed class GlobalPlatformEntity : EntityBasic, ISelectFilter
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

    /// <summary>
    /// IsDefault
    /// </summary>
    [SugarColumn(ColumnDescription = "是否默认")]
    public bool? IsDefault { get; set; }
}