using GameFrameX.Application.SeedData.System;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// 菜单组件与路径保持一致，所有菜单共用控制台作用域。
/// 路径目录沿用现有前端组件；超管可见性继续由 SuperAdminOnly 控制。
/// </summary>
[Collection("MenuSeedCatalog")]
public class GlobalMenuSeedAudiencePrefixGuardTests
{
    /// <summary>
    /// 种子叶子数量下限，防止种子被清空后守护测试空转通过。
    /// </summary>
    private const int MinimumLeafCount = 60;

    [Fact]
    public void LeafMenu_PathFirstSegment_MatchesComponentFirstSegment()
    {
        var leaves = CollectViewLeafMenus(GlobalMenuSeedData.BuildData());

        Assert.True(leaves.Count >= MinimumLeafCount, $"叶子菜单数量异常：{leaves.Count} < {MinimumLeafCount}");

        foreach (var menu in leaves)
        {
            var pathSegment = FirstSegment(menu.Path);
            var componentSegment = FirstSegment(menu.Component);

            Assert.True(
                pathSegment == componentSegment,
                $"菜单 {menu.Key} 的 Path 首段与 Component 首段不一致：Path={menu.Path}，Component={menu.Component}");
        }
    }

    [Fact]
    public void Menus_UseConsoleScopeAndRetainSuperAdminBoundary()
    {
        var menus = GlobalMenuSeedData.BuildData();
        Assert.All(menus, menu => Assert.Equal(MenuAppScopeEnum.Console, menu.AppScope));
        foreach (var menu in CollectViewLeafMenus(menus))
        {
            Assert.Equal(menu.SuperAdminOnly, FirstSegment(menu.Component) == "platform");
        }
    }

    [Fact]
    public void KnownDriftSamples_StayOnAudiencePrefixedValues()
    {
        var menus = GlobalMenuSeedData.BuildData();

        // C302 改名映射：logDiff/logVisit 与 gameDockerTagEntity，种子曾残留旧文件名
        var diffLogMenu = menus.Single(m => m.Key == "route:systemDiffLog");
        Assert.Equal("/tenant/system/log/logDiff/index", diffLogMenu.Component);

        var dockerMenu = menus.Single(m => m.Key == "route:gameDockerEntity");
        Assert.Equal("/app/opsCenter/gameDockerTagEntity/index", dockerMenu.Component);

        // userCenter 组件实际文件为 UserCenter.vue（大写 U），种子曾小写导致 dynamicImport 零匹配
        var userCenterMenu = menus.Single(m => m.Key == "route:systemUserCenter");
        Assert.Equal("/tenant/system/user/component/UserCenter", userCenterMenu.Component);
    }

    private static string FirstSegment(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.TrimStart('/').Split('/')[0];
    }

    /// <summary>
    /// 收集指向 views 组件的叶子菜单：Type=Menu 且 Component 非空、非 Layout（Dir 分组）、非 layout/ 前缀（iframe/外链壳）。
    /// </summary>
    private static List<GlobalMenuEntity> CollectViewLeafMenus(IReadOnlyCollection<GlobalMenuEntity> menus)
    {
        return menus
               .Where(m => m.Type == MenuTypeEnum.Menu)
               .Where(m =>
               {
                   var component = m.Component;
                   if (string.IsNullOrEmpty(component))
                   {
                       return false;
                   }

                   return component != "Layout" && !component.StartsWith("layout/", StringComparison.Ordinal);
               })
               .ToList();
    }
}
