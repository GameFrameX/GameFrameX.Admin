using GameFrameX.Application.SeedData.System;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Tests.System;

[Collection("MenuSeedCatalog")]
public class GlobalMenuSeedFlatStructureTests
{
    [Fact]
    public void OperationsRoots_AreStableEnabledDirectories()
    {
        var menus = GlobalMenuSeedData.BuildData();
        var roots = new[] { "serverCenter", "noticeCenter", "gmCenter", "commerceCenter", "clientDelivery", "audit" };
        foreach (var name in roots)
        {
            var menu = menus.Single(menu => menu.Key == $"route:{name}");
            Assert.Equal(StableSeedId.Menu(menu.Key!), menu.Id);
            Assert.Equal(0, menu.Pid);
            Assert.Equal(MenuTypeEnum.Dir, menu.Type);
            Assert.Equal($"/{name}", menu.Path);
            Assert.Equal($"message.menu.{name}", menu.TitleCode);
            Assert.Equal("Layout", menu.Component);
            Assert.True(menu.IsEnabled);
            Assert.False(menu.SuperAdminOnly);
            Assert.False(menu.IsDeleted);
        }
    }

    [Fact]
    public void AllMenuIdentitiesAndParents_RemainValidDuringMigration()
    {
        var menus = GlobalMenuSeedData.BuildData();
        Assert.Equal(menus.Count, menus.Select(menu => menu.Key).Distinct().Count());
        Assert.Equal(menus.Count, menus.Select(menu => menu.Id).Distinct().Count());
        var byIdentifier = menus.ToDictionary(menu => menu.Id);
        foreach (var menu in menus)
        {
            Assert.Equal(StableSeedId.Menu(menu.Key!), menu.Id);
            var ancestor = menu;
            var visited = new HashSet<long> { menu.Id };
            while (ancestor.Pid != 0)
            {
                Assert.True(byIdentifier.ContainsKey(ancestor.Pid), $"Missing parent for {ancestor.Key}");
                ancestor = byIdentifier[ancestor.Pid];
                Assert.True(visited.Add(ancestor.Id), $"Cycle at {ancestor.Key}");
            }
        }
    }

    [Fact(Skip = "C403 集成轮启用")]
    public void FinalNavigation_UsesNineOrderedRootsAndFlatViewPrefixes()
    {
        var menus = GlobalMenuSeedData.BuildData();
        var expectedKeys = new[]
        {
            "route:dashboard", "route:serverCenter", "route:noticeCenter", "route:gmCenter", "route:player",
            "route:commerceCenter", "route:clientDelivery", "route:audit", "route:system",
        };
        var prefixes = new[] { "dashboard", "serverCenter", "noticeCenter", "gmCenter", "playerCenter", "commerceCenter", "clientDelivery", "audit", "system" };
        var roots = menus.Where(menu => menu.Pid == 0).OrderBy(menu => menu.OrderNo).ToArray();
        Assert.Equal(expectedKeys, roots.Select(menu => menu.Key));
        Assert.Equal(prefixes.Select(prefix => $"/{prefix}"), roots.Select(menu => menu.Path));
        Assert.Equal(Enumerable.Range(0, 9).Select(index => index * 100), roots.Select(menu => menu.OrderNo));
        foreach (var menu in menus.Where(menu => menu.Type != MenuTypeEnum.Button))
        {
            Assert.Contains(menu.Path!.TrimStart('/').Split('/')[0], prefixes);
            if (menu.Type == MenuTypeEnum.Menu && menu.Component?.StartsWith('/') == true)
            {
                Assert.Contains(menu.Component.TrimStart('/').Split('/')[0], prefixes);
                Assert.Equal(menu.Path.Split('/')[1], menu.Component.Split('/')[1]);
            }
        }
    }
}
