using GameFrameX.Application.SeedData.System;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// C324（GFX-733）菜单种子受众前缀守护测试。
///
/// 受众判据（受众 = SuperAdminOnly，不是 AppScope；AppScope 是菜单上下文，与受众正交）：
/// <list type="bullet">
/// <item><c>/platform/</c>：SuperAdminOnly = true（无租户上下文可跑的超管页）</item>
/// <item><c>/app/</c>：AppScope = App（应用工作台业务中心）</item>
/// <item><c>/tenant/</c>：SuperAdminOnly = false 且 AppScope ≠ App（租户自管页）</item>
/// </list>
///
/// 断言范围：Type=Menu 且 Component 指向 views 组件的叶子节点；
/// Dir（Component="Layout"）与 iframe/外链（Component 以 layout/ 开头）不参与。
/// 起因：种子 Component 曾与前端 views（C302 改名）和运行库 DB 三方脱节，
/// 全新环境 seed 后前端 dynamicImport 零匹配导致白页。
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
    public void LeafMenu_FirstSegment_MatchesAudienceRule()
    {
        var leaves = CollectViewLeafMenus(GlobalMenuSeedData.BuildData());

        foreach (var menu in leaves)
        {
            var expected = DeriveAudienceSegment(menu);
            var pathSegment = FirstSegment(menu.Path);
            var componentSegment = FirstSegment(menu.Component);

            Assert.True(
                pathSegment == expected,
                $"菜单 {menu.Key} 的 Path 首段 {pathSegment} 与受众判据推导段 {expected} 不一致（SuperAdminOnly={menu.SuperAdminOnly}，AppScope={menu.AppScope}）：Path={menu.Path}");

            Assert.True(
                componentSegment == expected,
                $"菜单 {menu.Key} 的 Component 首段 {componentSegment} 与受众判据推导段 {expected} 不一致（SuperAdminOnly={menu.SuperAdminOnly}，AppScope={menu.AppScope}）：Component={menu.Component}");
        }
    }

    [Fact]
    public void KnownDriftSamples_StayOnAudiencePrefixedValues()
    {
        var menus = GlobalMenuSeedData.BuildData();

        // C302 后前端文件在 views/tenant/tenantSmsConfigEntity/，种子曾残留 /system/ 前缀（本守护的原始漂移样本）
        var smsMenu = menus.Single(m => m.Key == "route:tenantSmsConfigEntity");
        Assert.Equal("/tenant/system/tenantSmsConfigEntity", smsMenu.Path);
        Assert.Equal("/tenant/tenantSmsConfigEntity/index", smsMenu.Component);

        // C302 改名映射：logDiff/logVisit 与 gameDockerTagEntity，种子曾残留旧文件名
        var diffLogMenu = menus.Single(m => m.Key == "route:systemDiffLog");
        Assert.Equal("/tenant/system/log/logDiff/index", diffLogMenu.Component);

        var dockerMenu = menus.Single(m => m.Key == "route:gameDockerEntity");
        Assert.Equal("/app/opsCenter/gameDockerTagEntity/index", dockerMenu.Component);

        // userCenter 组件实际文件为 UserCenter.vue（大写 U），种子曾小写导致 dynamicImport 零匹配
        var userCenterMenu = menus.Single(m => m.Key == "route:systemUserCenter");
        Assert.Equal("/tenant/system/user/component/UserCenter", userCenterMenu.Component);
    }

    /// <summary>
    /// 按 SuperAdminOnly / AppScope 复合判据推导受众目录段。
    /// </summary>
    private static string DeriveAudienceSegment(GlobalMenuEntity menu)
    {
        if (menu.SuperAdminOnly)
        {
            return "platform";
        }

        if (menu.AppScope == MenuAppScopeEnum.App)
        {
            return "app";
        }

        return "tenant";
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
