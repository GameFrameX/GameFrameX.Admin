using GameFrameX.Entity.Routing;
using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Entity.Global;

/// <summary>
/// 系统租户套餐菜单表
/// </summary>
[SugarTable(null, "系统租户套餐菜单表")]
[GlobalTable]
[IncrementTable]
public class GlobalPackageMenuEntity : EntityBase
{
    /// <summary>
    /// 套餐Id
    /// </summary>
    [SugarColumn(ColumnDescription = "套餐Id", DefaultValue = "0")]
    public long PackageId { get; set; }

    /// <summary>
    /// 菜单Id
    /// </summary>
    [SugarColumn(ColumnDescription = "菜单Id", DefaultValue = "0")]
    public long MenuId { get; set; }

    /// <summary>
    /// 菜单
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(MenuId))]
    public GlobalMenuEntity Menu { get; set; } = null!;
}