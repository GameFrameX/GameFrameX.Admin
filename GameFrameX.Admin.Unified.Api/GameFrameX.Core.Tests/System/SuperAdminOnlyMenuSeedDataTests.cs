using GameFrameX.Core.Const.MenuSeed;

namespace GameFrameX.Core.Tests.System;

[Collection("MenuSeedCatalog")]
public class SuperAdminOnlyMenuSeedDataTests
{
    [Fact]
    public void SuperAdminOnlySubtrees_RemainProtectedUnderSystem()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();
        var menus = MenuSeedCatalog.Menus.ToDictionary(menu => menu.Id);
        var protectedMenuIds = MenuSeedCatalog.SuperAdminOnlySubtreeMenuIds;
        var systemMenu = menus.Values.Single(menu => menu.Key == "route:system");

        Assert.Equal(0, systemMenu.Pid);
        Assert.Equal("系统", systemMenu.Title);
        Assert.False(systemMenu.SuperAdminOnly);
        Assert.DoesNotContain(systemMenu.Id, protectedMenuIds);
        var protectedChildren = menus.Values.Where(menu => menu.Pid == systemMenu.Id && menu.SuperAdminOnly).ToList();
        Assert.NotEmpty(protectedChildren);
        Assert.All(protectedChildren, menu =>
        {
            Assert.Contains(menu.Id, protectedMenuIds);
            Assert.False(menu.AllowAdminWhenSuperAdminOnly);
        });
        Assert.Contains(protectedChildren, menu => menu.Key == "route:develop");
        Assert.Contains(protectedChildren, menu => menu.Key == "route:doc");
        Assert.Contains(protectedChildren, menu => menu.Key == "route:globalConfig");
    }

    [Fact]
    public void PlatformCaptchaConfigMenu_IsSuperAdminOnlyUnderSystem()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();
        var menus = MenuSeedCatalog.Menus.ToList();

        var systemMenu = menus.Single(menu => menu.Name == "system");
        var captchaMenu = menus.Single(menu => menu.Name == "platformCaptchaConfig");

        Assert.Equal(systemMenu.Id, captchaMenu.Pid);
        Assert.Equal("/system/platformCaptchaConfig", captchaMenu.Path);
        Assert.Equal("/system/platformCaptchaConfig/index", captchaMenu.Component);

        var definition = MenuSeedCatalog.AllDefinitions.Single(u => u.Key == "route:platformCaptchaConfig");
        Assert.True(definition.SuperAdminOnly);
        Assert.False(definition.AllowAdminWhenSuperAdminOnly);
    }

    [Fact]
    public void PlatformRateLimitConfigMenu_IsSuperAdminOnlyUnderGlobalConfig()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();
        var menus = MenuSeedCatalog.Menus.ToList();

        var globalConfigMenu = menus.Single(menu => menu.Name == "globalConfig");
        var rateLimitMenu = menus.Single(menu => menu.Name == "platformRateLimitConfig");

        Assert.Equal(globalConfigMenu.Id, rateLimitMenu.Pid);
        Assert.Equal("/system/platformRateLimitConfig", rateLimitMenu.Path);
        Assert.Equal("/system/platformRateLimitConfig/index", rateLimitMenu.Component);

        var definition = MenuSeedCatalog.AllDefinitions.Single(u => u.Key == "route:platformRateLimitConfig");
        Assert.True(definition.SuperAdminOnly);
        Assert.False(definition.AllowAdminWhenSuperAdminOnly);
    }
}