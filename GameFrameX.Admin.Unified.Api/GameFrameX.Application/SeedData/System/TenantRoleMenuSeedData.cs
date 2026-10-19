using GameFrameX.Core.Const;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.SqlSugar;

namespace GameFrameX.Application.SeedData.System;

/// <summary>
/// 系统角色菜单表 表种子数据。
/// </summary>
public class TenantRoleMenuSeedData : ISqlSugarEntitySeedData<TenantRoleMenuEntity>
{
    /// <summary>
    /// 种子数据。
    /// </summary>
    public IEnumerable<TenantRoleMenuEntity> HasData()
    {
        return TenantDefaultDataTemplates.RoleTemplates
                                         .SelectMany(role => MenuSeedCatalog.BuildRoleMenus(role.SeedId, role.Code))
                                         .GroupBy(u => new { u.RoleId, u.MenuId, })
                                         .Select(u => u.First())
                                         .ToList();
    }
}