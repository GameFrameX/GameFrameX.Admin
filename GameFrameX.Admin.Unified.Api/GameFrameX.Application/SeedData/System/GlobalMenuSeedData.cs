using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Global;
using GameFrameX.Foundation.Orm.Attribute;

namespace GameFrameX.Application.SeedData.System;

/// <summary>
/// 系统菜单表全局种子数据。
/// 菜单按业务域拆分到 GlobalMenuSeedData.&lt;Domain&gt;.cs 各 partial 文件；本文件仅保留 seed 入口与 <see cref="BuildData"/> 聚合。
/// </summary>
/// <remarks>
/// 权威收敛种子（C278）：global_menu.path / component 等菜单结构以本 seed 的 upsert 为唯一权威纠正源（spec §132/133），
/// 每次启动按 seed 值覆盖已有行；其余普通种子只补缺不覆盖。
/// </remarks>
[IncrementSeed]
public partial class GlobalMenuSeedData : ISqlSugarEntitySeedData<GlobalMenuEntity>
{
    /// <summary>
    /// 种子数据。
    /// </summary>
    public IEnumerable<GlobalMenuEntity> HasData()
    {
        return BuildData();
    }

    /// <summary>
    /// 构建全局菜单和按钮种子数据。
    /// </summary>
    public static IReadOnlyList<GlobalMenuEntity> BuildData()
    {
        var menus = new List<GlobalMenuEntity>();
        menus.AddRange(DashboardMenus());
        menus.AddRange(SystemMenus());
        menus.AddRange(PlatformMenus());
        menus.AddRange(GlobalConfigMenus());
        menus.AddRange(AppCenterMenus());
        menus.AddRange(PlayerMenus());
        menus.AddRange(TrustCenterMenus());
        menus.AddRange(GoodsMenus());
        menus.AddRange(OpsMenus());
        menus.AddRange(RedeemCodeMenus());
        menus.AddRange(WebShopMenus());
        menus.AddRange(AutomationMenus());
        menus.AddRange(ContentMenus());
        menus.AddRange(ContentRewardTemplateMenus());
        menus.AddRange(OnlineSocialMenus());
        menus.AddRange(OnlineMenus());
        menus.AddRange(OnlineMatchMenus());
        return menus;
    }
}