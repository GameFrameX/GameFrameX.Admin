using GameFrameX.Application.SeedData.System;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// 8 棵游戏业务目录路径前缀守卫：均使用新 Center 前缀（/appCenter、/playerCenter 等），
/// 且全量 seed 菜单中不存在任何 /game/ 子树前缀的 Path / Component 残留。
/// route:game 已彻底从 seed 移除（不再保留禁用隐藏行），故移除其存在性断言。
/// </summary>
[Collection("MenuSeedCatalog")]
public class GlobalMenuSeedDataLegacyGamePathZeroResidueTests
{
    private static readonly Dictionary<string, string> ExpectedBusinessDirPathByKey = new(StringComparer.Ordinal)
    {
        ["route:app"] = "/appCenter",
        ["route:player"] = "/playerCenter",
        ["route:goods"] = "/goodsCenter",
        ["route:redeemcode"] = "/redeemCenter",
        ["route:webShop"] = "/webShopCenter",
        ["route:content"] = "/contentCenter",
        ["route:ops"] = "/opsCenter",
        ["route:automation"] = "/automationCenter",
    };

    [Fact]
    public void EightBusinessDirs_AllUseNewCenterPrefix()
    {
        var menus = GlobalMenuSeedData.BuildData();

        foreach (var (key, expectedPath) in ExpectedBusinessDirPathByKey)
        {
            var menu = menus.Single(m => m.Key == key);
            Assert.Equal(expectedPath, menu.Path);
        }
    }

    [Fact]
    public void NoLegacyGameSubtreePrefix_AcrossAllSeedMenus()
    {
        var menus = GlobalMenuSeedData.BuildData();
        Assert.NotEmpty(menus);

        foreach (var menu in menus)
        {
            Assert.False(
                menu.Path != null && menu.Path.StartsWith("/game/", StringComparison.Ordinal),
                $"菜单 {menu.Key} 仍保留旧 /game/ 子树 Path：{menu.Path}");

            Assert.False(
                menu.Component != null && menu.Component.StartsWith("/game/", StringComparison.Ordinal),
                $"菜单 {menu.Key} 仍保留旧 /game/ 子树 Component：{menu.Component}");
        }
    }
}