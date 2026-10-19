using GameFrameX.Foundation.Orm.Entity;
using GameFrameX.Entity.Global;

namespace GameFrameX.Entity.Tenant;

/// <summary>
/// 系统角色菜单表
/// </summary>
[SugarTable(null, "租户库角色菜单表")]
[TenantTable]
[IncrementTable]
public class TenantRoleMenuEntity : EntityBase
{
    /// <summary>
    /// 角色Id
    /// </summary>
    [SugarColumn(ColumnDescription = "角色Id", DefaultValue = "0")]
    public long RoleId { get; set; }

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