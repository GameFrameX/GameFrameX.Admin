using GameFrameX.Core.Const;
using GameFrameX.Core.Const.MenuSeed;

namespace GameFrameX.Core.Tests.System;

[Collection("MenuSeedCatalog")]
public class SuperAdminOnlyMenuSeedDataTests
{
    [Fact]
    public void SuperAdminOnlyMenuRootIds_IncludePlatformConfigurationRoot()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();
        var menus = MenuSeedCatalog.Menus.ToDictionary(menu => menu.Id);
        var reservedRootIds = CommonConst.SuperAdminOnlyMenuRootIds.ToHashSet();

        var platformMenu = menus.Values.Single(menu => menu.Name == "platform");

        Assert.Equal(0, platformMenu.Pid);
        Assert.Equal("平台管理", platformMenu.Title);
        Assert.Contains(platformMenu.Id, reservedRootIds);
    }

    [Fact]
    public void PlatformCaptchaConfigMenu_IsSuperAdminOnlyUnderPlatform()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();
        var menus = MenuSeedCatalog.Menus.ToList();

        var platformMenu = menus.Single(menu => menu.Name == "platform");
        var captchaMenu = menus.Single(menu => menu.Name == "platformCaptchaConfig");

        Assert.Equal(platformMenu.Id, captchaMenu.Pid);
        Assert.Equal("/platform/captcha", captchaMenu.Path);
        Assert.Equal("/platform/platformCaptchaConfig/index", captchaMenu.Component);

        var definition = MenuSeedCatalog.AllDefinitions.Single(u => u.Key == "route:platformCaptchaConfig");
        Assert.True(definition.SuperAdminOnly);
        Assert.False(definition.AllowTenantWhenSuperAdminOnly);
    }

    [Fact]
    public void PlatformRateLimitConfigMenu_IsSuperAdminOnlyUnderGlobalConfig()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();
        var menus = MenuSeedCatalog.Menus.ToList();

        var globalConfigMenu = menus.Single(menu => menu.Name == "globalConfig");
        var rateLimitMenu = menus.Single(menu => menu.Name == "platformRateLimitConfig");

        Assert.Equal(globalConfigMenu.Id, rateLimitMenu.Pid);
        Assert.Equal("/platform/global/platformRateLimitConfig", rateLimitMenu.Path);
        Assert.Equal("/platform/global/platformRateLimitConfig/index", rateLimitMenu.Component);

        var definition = MenuSeedCatalog.AllDefinitions.Single(u => u.Key == "route:platformRateLimitConfig");
        Assert.True(definition.SuperAdminOnly);
        Assert.False(definition.AllowTenantWhenSuperAdminOnly);
    }
}