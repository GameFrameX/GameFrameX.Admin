using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.Tests.System;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Tests.TenantMenuSync;

/// <summary>
/// C353 T1：CatalogVersion 只对引擎消费字段敏感（Key/ParentKey/Type/SyncRoles/SuperAdminOnly/AllowTenantWhenSuperAdminOnly/AppScope），
/// 纯展示字段（Title/Icon/OrderNo）变化不得推动版本（否则每次美化菜单都触发全量租户重同步）。
/// </summary>
[Collection("MenuSeedCatalog")]
public class TenantMenuSyncCatalogVersionTests
{
    [Fact]
    public void CatalogVersion_IsDeterministicAcrossCalls()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();

        var first = MenuSeedCatalog.CatalogVersion;
        var second = MenuSeedCatalog.CatalogVersion;
        Assert.Equal(first, second);
    }

    [Fact]
    public void CatalogVersion_IsOrderIndependent()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();

        var ordered = MenuSeedCatalog.AllDefinitions.OrderBy(u => u.Key, StringComparer.Ordinal).ToArray();
        var shuffled = MenuSeedCatalog.AllDefinitions.OrderByDescending(u => u.Key, StringComparer.Ordinal).ToArray();

        Assert.Equal(
            MenuSeedCatalog.ComputeCatalogVersion(ordered),
            MenuSeedCatalog.ComputeCatalogVersion(shuffled));
    }

    [Fact]
    public void CatalogVersion_UnchangedWhenOnlyPresentationFieldsChange()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();

        var retitled = MenuSeedCatalog.AllDefinitions
                                      .Select(u => u with { Title = u.Title + "-renamed", Icon = "fa-solid fa-x", OrderNo = u.OrderNo + 999 })
                                      .ToArray();

        Assert.Equal(
            MenuSeedCatalog.ComputeCatalogVersion(MenuSeedCatalog.AllDefinitions.ToArray()),
            MenuSeedCatalog.ComputeCatalogVersion(retitled));
    }

    [Theory]
    [InlineData("syncRoles")]
    [InlineData("superAdminOnly")]
    [InlineData("allowTenantWhenSuperAdminOnly")]
    [InlineData("parentKey")]
    [InlineData("appScope")]
    public void CatalogVersion_ChangesWhenEngineConsumedFieldChanges(string field)
    {
        MenuSeedCatalogTestData.LoadSampleMenus();

        var baseline = MenuSeedCatalog.ComputeCatalogVersion(MenuSeedCatalog.AllDefinitions.ToArray());
        var modified = ApplyFieldChange(MenuSeedCatalog.AllDefinitions.ToArray(), field);
        var after = MenuSeedCatalog.ComputeCatalogVersion(modified);

        Assert.NotEqual(baseline, after);
    }

    private static MenuSeedDefinition[] ApplyFieldChange(MenuSeedDefinition[] definitions, string field)
    {
        var target = definitions.First(u => u.Type == MenuTypeEnum.Menu && !u.SuperAdminOnly);
        MenuSeedDefinition changed = field switch
        {
            "syncRoles" => target with { SyncRoles = target.SyncRoles == TenantMenuSyncRole.None ? TenantMenuSyncRole.TenantAdmin : TenantMenuSyncRole.None },
            "superAdminOnly" => target with { SuperAdminOnly = !target.SuperAdminOnly },
            "allowTenantWhenSuperAdminOnly" => target with { AllowTenantWhenSuperAdminOnly = !target.AllowTenantWhenSuperAdminOnly },
            "parentKey" => target with { ParentKey = "route:another-parent" },
            "appScope" => target with { AppScope = target.AppScope == MenuAppScopeEnum.App ? MenuAppScopeEnum.Platform : MenuAppScopeEnum.App },
            _ => throw new ArgumentOutOfRangeException(nameof(field)),
        };

        return definitions.Select(u => ReferenceEquals(u, target) ? changed : u).ToArray();
    }
}
