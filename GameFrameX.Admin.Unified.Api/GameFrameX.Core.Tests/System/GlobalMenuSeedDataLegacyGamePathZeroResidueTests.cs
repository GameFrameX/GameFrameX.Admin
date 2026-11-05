using GameFrameX.Application.SeedData.System;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// 全量种子不得重新使用已退役的 /game/ 路径前缀。
/// </summary>
[Collection("MenuSeedCatalog")]
public class GlobalMenuSeedDataLegacyGamePathZeroResidueTests
{
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