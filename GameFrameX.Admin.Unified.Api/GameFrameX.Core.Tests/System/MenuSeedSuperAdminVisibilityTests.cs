using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Entity.Enum;
using Xunit.Abstractions;

namespace GameFrameX.Core.Tests.System;

[Collection("MenuSeedCatalog")]
public class MenuSeedSuperAdminVisibilityTests
{
    private readonly ITestOutputHelper _output;

    public MenuSeedSuperAdminVisibilityTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ProtectedSubtrees_IncludeDirectoryAncestorsWithoutTenantLeaves()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();
        var definitions = MenuSeedCatalog.AllDefinitions.ToDictionary(definition => definition.Key);
        var visibleIds = MenuSeedCatalog.SuperAdminVisibleMenuIds.ToHashSet();

        foreach (var definition in definitions.Values.Where(definition => definition.SuperAdminOnly))
        {
            Assert.Contains(StableSeedId.Menu(definition.Key), visibleIds);
            var parentKey = definition.ParentKey;
            while (parentKey != null)
            {
                var parent = definitions[parentKey];
                Assert.Contains(StableSeedId.Menu(parent.Key), visibleIds);
                parentKey = parent.ParentKey;
            }
        }

        var publicKeys = new HashSet<string>(StringComparer.Ordinal) { "route:dashboard", "route:home" };
        Assert.All(definitions.Values.Where(definition =>
            definition.Type == MenuTypeEnum.Menu && visibleIds.Contains(StableSeedId.Menu(definition.Key))),
            definition => Assert.True(definition.SuperAdminOnly || publicKeys.Contains(definition.Key), definition.Key));

        var protectedIds = MenuSeedCatalog.SuperAdminOnlySubtreeMenuIds;
        Assert.All(definitions.Values.Where(definition => visibleIds.Contains(StableSeedId.Menu(definition.Key)) &&
            !protectedIds.Contains(StableSeedId.Menu(definition.Key)) && !publicKeys.Contains(definition.Key)),
            definition => Assert.Equal(MenuTypeEnum.Dir, definition.Type));
    }

    [Fact]
    public void VisibleNavigation_ContainsOnlyDashboardAndSystemRoots()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();
        var visibleIds = MenuSeedCatalog.SuperAdminVisibleMenuIds.ToHashSet();
        var visibleMenus = MenuSeedCatalog.Menus.Where(menu => visibleIds.Contains(menu.Id)).ToList();
        var roots = visibleMenus.Where(menu => menu.Pid == 0).OrderBy(menu => menu.OrderNo).ToList();

        Assert.Equal(new[] { "route:dashboard", "route:system" }, roots.Select(menu => menu.Key));
        _output.WriteLine("超管可见根：" + string.Join("、", roots.Select(menu => menu.Title)));
        var system = roots.Single(menu => menu.Key == "route:system");
        var children = visibleMenus.Where(menu => menu.Pid == system.Id).OrderBy(menu => menu.OrderNo).ToList();
        Assert.Equal(new[] { "route:develop", "route:doc", "route:globalConfig" },
            children.Where(menu => menu.Type == MenuTypeEnum.Dir).Select(menu => menu.Key).OrderBy(key => key));
        Assert.Equal(new[]
        {
            "route:platformCaptchaConfig", "route:systemConfig", "route:systemDict",
            "route:systemEmailProviderEntity", "route:systemJob", "route:systemMenu", "route:systemOpenAccess",
        }, children.Where(menu => menu.Type == MenuTypeEnum.Menu).Select(menu => menu.Key).OrderBy(key => key));
        Assert.All(children, menu => Assert.True(menu.SuperAdminOnly));
        _output.WriteLine("系统直接子级：" + string.Join("、", children.Select(menu => menu.Title)));
    }
}
