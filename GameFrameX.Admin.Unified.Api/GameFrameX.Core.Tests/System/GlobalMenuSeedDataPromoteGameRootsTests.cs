using GameFrameX.Application.SeedData.System;
using GameFrameX.Core.Const.MenuSeed;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// 8 棵游戏业务顶层根（route:app/player/goods/ops/redeemcode/webShop/automation/content）
/// 的结构守卫：Pid=0、启用、StableSeedId/Key 稳定，以及各根的租户角色可见性 SyncRoles 边界。
/// 历史背景：这些根原挂在 route:game 下，C104 提升为顶层根；route:game 已彻底从 seed 移除
/// （不再保留禁用隐藏行），故移除所有 route:game 存在性断言，仅保留业务根的结构与可见性契约。
/// </summary>
[Collection("MenuSeedCatalog")]
public class GlobalMenuSeedDataPromoteGameRootsTests
{
    private static readonly string[] PromotedRootKeys =
    {
        "route:app",
        "route:player",
        "route:goods",
        "route:ops",
        "route:redeemcode",
        "route:webShop",
        "route:automation",
        "route:content",
    };

    // 与 GlobalMenuSeedData 中各根的 SyncRoles 字面量保持一致；任何漂移都会让本测试失败。
    private static readonly Dictionary<string, TenantMenuSyncRole> ExpectedSyncRolesByKey =
        new(StringComparer.Ordinal)
        {
            ["route:app"] = TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.ServerOps | TenantMenuSyncRole.PaymentOps | TenantMenuSyncRole.DataAnalyst | TenantMenuSyncRole.AppConfigAdmin,
            ["route:player"] = TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.OpsManager | TenantMenuSyncRole.GmSupport | TenantMenuSyncRole.PaymentOps | TenantMenuSyncRole.DataAnalyst,
            ["route:goods"] = TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.OpsManager | TenantMenuSyncRole.PaymentOps | TenantMenuSyncRole.MarketingOps | TenantMenuSyncRole.DataAnalyst,
            ["route:ops"] = TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.ServerOps | TenantMenuSyncRole.DataAnalyst,
            ["route:redeemcode"] = TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.OpsManager | TenantMenuSyncRole.MarketingOps | TenantMenuSyncRole.DataAnalyst,
            ["route:webShop"] = TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.OpsManager | TenantMenuSyncRole.PaymentOps | TenantMenuSyncRole.DataAnalyst,
            ["route:automation"] = TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.ServerOps,
            ["route:content"] = TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.OpsManager | TenantMenuSyncRole.GmSupport | TenantMenuSyncRole.ContentOps | TenantMenuSyncRole.MarketingOps | TenantMenuSyncRole.DataAnalyst,
        };

    [Fact]
    public void GameBusinessMenus_AreTopLevelEnabledRoots()
    {
        var menus = GlobalMenuSeedData.BuildData();

        foreach (var key in PromotedRootKeys)
        {
            var menu = menus.Single(m => m.Key == key);
            Assert.Equal(0, menu.Pid);
            Assert.True(menu.IsEnabled, $"顶层业务根 {key} 应保持启用，否则租户看不见。");
        }
    }

    [Fact]
    public void PromotedRoots_PreserveTenantVisibilitySyncRoles()
    {
        var menus = GlobalMenuSeedData.BuildData();

        foreach (var (key, expected) in ExpectedSyncRolesByKey)
        {
            var menu = menus.Single(m => m.Key == key);
            Assert.Equal((int)expected, menu.SyncRoles);
        }
    }

    [Fact]
    public void PromotedRoots_StableSeedIdAndKeyStructure_Unchanged()
    {
        var menus = GlobalMenuSeedData.BuildData();

        foreach (var key in PromotedRootKeys)
        {
            var menu = menus.Single(m => m.Key == key);
            Assert.Equal(key, menu.Key);
            Assert.Equal(StableSeedId.Menu(key), menu.Id);
        }
    }
}