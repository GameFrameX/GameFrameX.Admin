using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.Service.TenantMenuSync;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Tests.TenantMenuSync;

/// <summary>
/// C353 T2：套餐闭包计算器矩阵。闭包 = 清单 ∪ 祖先 ∪ 后代（按钮随父页面）∩ 种子域 − SAO 子树。
/// </summary>
public class TenantMenuSyncClosureCalculatorTests
{
    private const string RootKey = "route:root";
    private const string PageKey = "route:page";
    private const string ButtonKey = "page:btn";
    private const string OtherRootKey = "route:other";

    private static MenuSeedDefinition Def(string key, string? parentKey, MenuTypeEnum type, TenantMenuSyncRole roles, bool superAdminOnly = false)
    {
        return new MenuSeedDefinition(
            key, parentKey, type, key, key, key, null, null, key, null, null, null,
            false, false, true, false, 100, true, null, 0, 0, roles, superAdminOnly, false, MenuAppScopeEnum.App);
    }

    /// <summary>
    /// 两棵树：root→page→btn；other 独立根；saoRoot 为 SuperAdmin-only 根下挂 saoPage/saoBtn。
    /// </summary>
    private static List<MenuSeedDefinition> BuildDefinitions()
    {
        return new List<MenuSeedDefinition>
        {
            Def(RootKey, null, MenuTypeEnum.Dir, TenantMenuSyncRole.TenantAdmin),
            Def(PageKey, RootKey, MenuTypeEnum.Menu, TenantMenuSyncRole.TenantAdmin),
            Def(ButtonKey, PageKey, MenuTypeEnum.Button, TenantMenuSyncRole.TenantAdmin),
            Def(OtherRootKey, null, MenuTypeEnum.Dir, TenantMenuSyncRole.TenantAdmin),
            Def("route:sao", null, MenuTypeEnum.Dir, TenantMenuSyncRole.None, superAdminOnly: true),
            Def("route:saoPage", "route:sao", MenuTypeEnum.Menu, TenantMenuSyncRole.None, superAdminOnly: true),
            Def("saoPage:btn", "route:saoPage", MenuTypeEnum.Button, TenantMenuSyncRole.None, superAdminOnly: true),
        };
    }

    [Fact]
    public void NullPackageId_ReturnsNoCeiling()
    {
        var ceiling = TenantMenuSyncClosureCalculator.Compute(null, packageUsable: false, Array.Empty<long>(), BuildDefinitions());
        Assert.Equal(TenantMenuSyncCeilingKind.None, ceiling.Kind);
    }

    [Fact]
    public void UnusablePackage_ReturnsDangling()
    {
        var ceiling = TenantMenuSyncClosureCalculator.Compute(100L, packageUsable: false, new[] { StableSeedId.Menu(PageKey) }, BuildDefinitions());
        Assert.Equal(TenantMenuSyncCeilingKind.Dangling, ceiling.Kind);
    }

    [Fact]
    public void UsablePackageWithEmptyList_ReturnsBoundedEmpty()
    {
        var ceiling = TenantMenuSyncClosureCalculator.Compute(100L, packageUsable: true, Array.Empty<long>(), BuildDefinitions());
        Assert.Equal(TenantMenuSyncCeilingKind.Bounded, ceiling.Kind);
        Assert.Empty(ceiling.MenuIds);
    }

    [Fact]
    public void ListedPage_PullsAncestorsAndDescendantButtons()
    {
        var ceiling = TenantMenuSyncClosureCalculator.Compute(100L, packageUsable: true, new[] { StableSeedId.Menu(PageKey) }, BuildDefinitions());

        Assert.Equal(TenantMenuSyncCeilingKind.Bounded, ceiling.Kind);
        Assert.Contains(StableSeedId.Menu(PageKey), ceiling.MenuIds);
        Assert.Contains(StableSeedId.Menu(RootKey), ceiling.MenuIds);
        Assert.Contains(StableSeedId.Menu(ButtonKey), ceiling.MenuIds);
        Assert.DoesNotContain(StableSeedId.Menu(OtherRootKey), ceiling.MenuIds);
    }

    [Fact]
    public void ListedButton_AncestorClosurePullsParentPage()
    {
        var ceiling = TenantMenuSyncClosureCalculator.Compute(100L, packageUsable: true, new[] { StableSeedId.Menu(ButtonKey) }, BuildDefinitions());

        Assert.Contains(StableSeedId.Menu(PageKey), ceiling.MenuIds);
        Assert.Contains(StableSeedId.Menu(RootKey), ceiling.MenuIds);
    }

    [Fact]
    public void SuperAdminOnlySubtree_IsDefensivelyRemoved()
    {
        var definitions = BuildDefinitions();
        var listed = new[]
        {
            StableSeedId.Menu("route:sao"),
            StableSeedId.Menu("route:saoPage"),
            StableSeedId.Menu("saoPage:btn"),
            StableSeedId.Menu(PageKey),
        };

        var ceiling = TenantMenuSyncClosureCalculator.Compute(100L, packageUsable: true, listed, definitions);

        Assert.DoesNotContain(StableSeedId.Menu("route:sao"), ceiling.MenuIds);
        Assert.DoesNotContain(StableSeedId.Menu("route:saoPage"), ceiling.MenuIds);
        Assert.DoesNotContain(StableSeedId.Menu("saoPage:btn"), ceiling.MenuIds);
        Assert.Contains(StableSeedId.Menu(PageKey), ceiling.MenuIds);
    }

    [Fact]
    public void NonSeedMenuIds_InPackageListAreIgnored()
    {
        var ceiling = TenantMenuSyncClosureCalculator.Compute(100L, packageUsable: true, new[] { 999999L }, BuildDefinitions());

        Assert.Equal(TenantMenuSyncCeilingKind.Bounded, ceiling.Kind);
        Assert.Empty(ceiling.MenuIds);
    }
}
