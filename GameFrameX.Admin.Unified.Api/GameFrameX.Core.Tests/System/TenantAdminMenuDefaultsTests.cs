using System.Text.RegularExpressions;
using GameFrameX.Application.SeedData.System;
using GameFrameX.Core.Const;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Tests.System;

[Collection("MenuSeedCatalog")]
public class TenantAdminMenuDefaultsTests
{
    [Fact]
    public void SystemAdminSeedRole_UsesSharedTenantAdminDefaultMenus()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();
        var adminRole = TenantDefaultDataTemplates.RoleTemplates.Single(u => u.Code == TenantDefaultDataTemplates.AdminRoleCode);
        var systemAdminMenuIds = new TenantRoleMenuSeedData().HasData()
                                                             .Where(u => u.RoleId == adminRole.SeedId)
                                                             .Select(u => u.MenuId)
                                                             .OrderBy(u => u)
                                                             .ToArray();

        var defaultMenuIds = TenantAdminMenuDefaults.MenuIds
                                                    .OrderBy(u => u)
                                                    .ToArray();

        Assert.Equal(defaultMenuIds, systemAdminMenuIds);
    }

    [Fact]
    public void MenuSeedCatalog_LoadsUniqueDbKeysPermissionsAndValidParents()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();
        var definitions = MenuSeedCatalog.AllDefinitions;

        Assert.Equal(definitions.Count, definitions.Select(u => u.Key).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(definitions.Count, definitions.Select(u => StableSeedId.Menu(u.Key)).Distinct().Count());

        var permissions = definitions.Where(u => !string.IsNullOrWhiteSpace(u.Permission)).Select(u => u.Permission!).ToArray();
        Assert.Equal(permissions.Length, permissions.Distinct(StringComparer.Ordinal).Count());

        var definitionKeys = definitions.Select(u => u.Key).ToHashSet(StringComparer.Ordinal);
        var missingParents = definitions
                             .Where(u => u.ParentKey != null && !definitionKeys.Contains(u.ParentKey))
                             .Select(u => u.Key)
                             .ToArray();

        Assert.Empty(missingParents);
    }

    [Fact]
    public void TenantAdminDefaultMenus_IncludeDbSyncedButtonsAndTenantReadableHelpers()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();
        var menusByPermission = MenuSeedCatalog.AllDefinitions
                                               .Where(u => !string.IsNullOrWhiteSpace(u.Permission))
                                               .ToDictionary(u => u.Permission!, u => StableSeedId.Menu(u.Key), StringComparer.Ordinal);
        var tenantMenuIds = TenantAdminMenuDefaults.MenuIds.ToHashSet();

        // ownRoleList/ownExtOrgList 为保留的菜单权限码契约（GFX-710 豁免）；C368 起 Permission 由
        // MenuSeedRouteName.Of 推导，大小写与参数化路由段随规范形态（select / {userId}）。
        foreach (var permission in new[] { "tenantOrganization:list", "globalPlatform:select", "globalLanguage:select", "tenantUser:ownRoleList:{userId}", "tenantUser:ownExtOrgList:{userId}", })
        {
            Assert.True(menusByPermission.TryGetValue(permission, out var menuId), $"缺少 DB 菜单权限：{permission}");
            Assert.Contains(menuId, tenantMenuIds);
        }
    }

    [Fact]
    public void TenantAdminDefaultMenus_IncludeButtonsUnderTenantVisiblePages()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();
        var tenantMenuIds = TenantAdminMenuDefaults.MenuIds.ToHashSet();
        var tenantPageKeys = MenuSeedCatalog.AllDefinitions
                                            .Where(u => u.Type != MenuTypeEnum.Button && u.SyncRoles.HasFlag(TenantMenuSyncRole.TenantAdmin))
                                            .Select(u => u.Key)
                                            .ToHashSet(StringComparer.Ordinal);

        var missingButtons = MenuSeedCatalog.AllDefinitions
                                            .Where(u => u.Type == MenuTypeEnum.Button)
                                            .Where(u => u.ParentKey != null && tenantPageKeys.Contains(u.ParentKey))
                                            .Where(u => !u.SuperAdminOnly)
                                            .Where(u => !tenantMenuIds.Contains(StableSeedId.Menu(u.Key)))
                                            .Select(u => $"{u.ParentKey}/{u.Permission ?? u.Key}")
                                            .ToArray();

        Assert.Empty(missingButtons);
    }

    [Fact]
    public void TenantAdvertisementCallbackConfigMenu_IsVisibleToTenantAdminAndAppConfigAdmin()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();
        var expectedKeys = new[]
        {
            "route:tenantAdvertisementCallbackConfigEntity",
            "tenantAdvertisementCallbackConfigEntity:page",
            "tenantAdvertisementCallbackConfigEntity:add",
            "tenantAdvertisementCallbackConfigEntity:update",
            "tenantAdvertisementCallbackConfigEntity:delete",
            "tenantAdvertisementCallbackConfigEntity:detail",
            "tenantAdvertisementCallbackConfigEntity:providers",
            "tenantAdvertisementCallbackConfigEntity:schema",
        };
        var expectedIds = expectedKeys.Select(StableSeedId.Menu).ToArray();
        var tenantAdminIds = MenuSeedCatalog.GetMenuIdsForRoleCode(TenantDefaultDataTemplates.AdminRoleCode);
        var appConfigAdminIds = MenuSeedCatalog.GetMenuIdsForRoleCode("app_config_admin");

        foreach (var menuId in expectedIds)
        {
            Assert.Contains(menuId, tenantAdminIds);
            Assert.Contains(menuId, appConfigAdminIds);
        }
    }

    [Fact]
    public void TenantAdminDefaultMenus_DoNotExposeSuperAdminOnlyPages()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();
        var tenantMenuIds = TenantAdminMenuDefaults.MenuIds.ToHashSet();
        var invalidDefinitions = MenuSeedCatalog.AllDefinitions
                                                .Where(u => u.SuperAdminOnly && !u.AllowTenantWhenSuperAdminOnly)
                                                .Where(u => tenantMenuIds.Contains(StableSeedId.Menu(u.Key)))
                                                .Select(u => u.Key)
                                                .ToArray();

        Assert.Empty(invalidDefinitions);
    }

    [Fact]
    public void PlatformJwtConfigMenu_IsSuperAdminOnlyAndNotTenantReadable()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();
        var definitions = MenuSeedCatalog.AllDefinitions;
        var page = definitions.Single(u => u.Key == "route:platformJwtConfig");
        var detail = definitions.Single(u => u.Permission == "platformJwtConfig:detail");
        var update = definitions.Single(u => u.Permission == "platformJwtConfig:update");
        var tenantMenuIds = TenantAdminMenuDefaults.MenuIds.ToHashSet();

        Assert.Equal("route:globalConfig", page.ParentKey);
        Assert.True(page.SuperAdminOnly);
        Assert.False(page.AllowTenantWhenSuperAdminOnly);
        Assert.True(detail.SuperAdminOnly);
        Assert.True(update.SuperAdminOnly);
        Assert.False(detail.AllowTenantWhenSuperAdminOnly);
        Assert.False(update.AllowTenantWhenSuperAdminOnly);
        Assert.DoesNotContain(StableSeedId.Menu(page.Key), tenantMenuIds);
        Assert.DoesNotContain(StableSeedId.Menu(detail.Key), tenantMenuIds);
        Assert.DoesNotContain(StableSeedId.Menu(update.Key), tenantMenuIds);
    }

    [Fact]
    public void TenantReadableHelperButtons_AreNotRemovedBySuperAdminOnlyBoundary()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();
        var helperMenuIds = MenuSeedCatalog.TenantReadableSuperAdminOnlyMenuIds;
        Assert.NotEmpty(helperMenuIds);

        foreach (var permission in new[] { "globalPlatform:select", "globalLanguage:select", })
        {
            var helper = MenuSeedCatalog.AllDefinitions.Single(u => u.Permission == permission);
            Assert.True(helper.SuperAdminOnly);
            Assert.True(helper.AllowTenantWhenSuperAdminOnly);
            Assert.Contains(StableSeedId.Menu(helper.Key), helperMenuIds);
            Assert.Contains(StableSeedId.Menu(helper.Key), TenantAdminMenuDefaults.MenuIds);
        }
    }

    [Fact]
    public void MenuSeedSource_NoLongerUsesStaticDefinitionFilesOrAttributes()
    {
        var sourceRoot = FindSourceRoot();
        var forbiddenNames = new[]
        {
            "MenuSeedDefinitions",
            "MenuSeedAttributeDefinitions",
            "AdminMenuAttribute",
            "AdminMenuButtonAttribute",
            "MenuButtonPrefixAttribute",
            "RequireMenuButtonAttribute",
        };

        var sourceFiles = Directory.EnumerateFiles(sourceRoot.FullName, "*.cs", SearchOption.AllDirectories)
                                   .Where(u => !u.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                                   .Where(u => !u.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                                   .ToArray();

        foreach (var forbiddenName in forbiddenNames)
        {
            Assert.DoesNotContain(sourceFiles, file => Path.GetFileNameWithoutExtension(file).Contains(forbiddenName, StringComparison.Ordinal));
        }

        var productionFiles = sourceFiles.Where(u => !u.Contains($"{Path.DirectorySeparatorChar}GameFrameX.Core.Tests{Path.DirectorySeparatorChar}", StringComparison.Ordinal));
        foreach (var file in productionFiles)
        {
            var text = File.ReadAllText(file);
            foreach (var forbiddenName in forbiddenNames)
            {
                Assert.DoesNotContain(forbiddenName, text, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public void RoleAuthorizationFiles_DoNotContainPermissionRouteStrings()
    {
        var sourceRoot = FindSourceRoot();
        var files = new[]
        {
            sourceRoot.Combine("GameFrameX.Application/SeedData/System/TenantRoleMenuSeedData.cs"),
            sourceRoot.Combine("GameFrameX.Core/Const/TenantAdminMenuDefaults.cs"),
            sourceRoot.Combine("GameFrameX.Core/Service/Tenant/DefaultTenantDatabaseSeeder.cs"),
        };

        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            Assert.DoesNotMatch(new Regex(@"""[A-Za-z][A-Za-z0-9_]*:[A-Za-z][A-Za-z0-9_]*""", RegexOptions.Compiled), text);
        }
    }

    private static DirectoryInfo FindSourceRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameFrameX.Admin.Unified.slnx")))
        {
            directory = directory.Parent;
        }

        return directory ?? throw new DirectoryNotFoundException("找不到后端源码根目录。");
    }
}