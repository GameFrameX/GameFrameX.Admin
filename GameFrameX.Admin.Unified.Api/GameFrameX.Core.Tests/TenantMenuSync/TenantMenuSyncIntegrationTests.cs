using GameFrameX.Core.Const;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.Service.TenantMenuSync;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;
using SqlSugar;

namespace GameFrameX.Core.Tests.TenantMenuSync;

/// <summary>
/// C353 T8：Applier 端到端集成（真实 SqlSugar SQLite 主库 + 租户库）。
/// 覆盖：物理表名、全量收敛、两轮幂等、降级删除、null 套餐不夹逼、悬空跳过置 failed、空清单全量删除。
/// </summary>
[Collection("MenuSeedCatalog")]
public class TenantMenuSyncIntegrationTests : IDisposable
{
    private readonly TenantMenuSyncTestHost _host = new();

    [Fact]
    public void StateTable_UsesPlatformPhysicalName()
    {
        // 物理命名链路：[PlatformTable] → system_ 前缀 + _entity 后缀
        var tables = _host.MainDb.DbMaintenance.GetTableInfoList(false).Select(u => u.Name).ToList();
        Assert.Contains("system_tenant_menu_sync_state_entity", tables);
        Assert.Contains("system_tenant_entity", tables);
        Assert.Contains("global_package_menu_entity", tables);
    }

    [Fact]
    public async Task FullConvergence_ThenIdempotentSecondRun()
    {
        // 套餐含全部种子菜单 ⇒ ceiling = 全集 − SAO 子树（闭包防御剔除）；
        // 模板角色有效期望 = C189 期望集 ∩ ceiling（tenant-readable SAO 辅助按钮被 ceiling 防御挡下，与 T3 单测语义一致）
        var allIds = MenuSeedCatalog.AllDefinitions.Select(u => StableSeedId.Menu(u.Key)).ToArray();
        await _host.SeedPackageAsync(isEnabled: true, allIds);
        await _host.SeedTenantAsync(TenantMenuSyncTestHost.PackageId);
        await _host.SeedTenantRolesAsync();

        var applier = _host.BuildApplier();
        var tenant = await _host.MainDb.Queryable<SystemTenantEntity>().FirstAsync(u => u.Id == TenantMenuSyncTestHost.TenantId);
        var first = await applier.SyncTenantAsync(tenant, "test:full");

        Assert.True(first.Succeeded);
        Assert.True(first.InsertCount > 0);
        var saoSubtree = MenuSeedCatalog.GetSuperAdminOnlySubtreeMenuIds(MenuSeedCatalog.AllDefinitions);
        var expectedAdmin = MenuSeedCatalog.GetSyncedMenuIds(MenuSeedCatalog.AllDefinitions, TenantMenuSyncRole.TenantAdmin)
                                           .Where(u => !saoSubtree.Contains(u))
                                           .OrderBy(u => u)
                                           .ToArray();
        var actualAdmin = await _host.GetRoleMenuIdsAsync(TenantMenuSyncTestHost.AdminRoleId);
        Assert.Equal(expectedAdmin, actualAdmin.OrderBy(u => u).ToArray());
        Assert.Empty(await _host.GetRoleMenuIdsAsync(TenantMenuSyncTestHost.CustomRoleId));

        var state = await _host.GetStateAsync();
        Assert.NotNull(state);
        Assert.Equal(TenantMenuSyncStatusEnum.Succeeded, state!.Status);
        Assert.Equal(MenuSeedCatalog.CatalogVersion, state.LastCatalogVersion);

        var opLogCount = await _host.MainDb.Queryable<Entity.Log.SystemLogOperationEntity>()
                                    .CountAsync(u => u.ControllerName == "TenantMenuSync");
        Assert.Equal(1, opLogCount);

        // 第二轮幂等：零插入零删除
        var second = await applier.SyncTenantAsync(tenant, "test:idempotent");
        Assert.True(second.Succeeded);
        Assert.Equal(0, second.InsertCount);
        Assert.Equal(0, second.DeleteCount);
        Assert.Equal(actualAdmin.Count, await _host.CountRoleMenusAsync(TenantMenuSyncTestHost.AdminRoleId));
    }

    [Fact]
    public async Task PackageDowngrade_SurplusDeletedIncludingCustomClamp()
    {
        // 全量套餐收敛后，降级为只含一个根目录 ⇒ 模板与自定义角色的超闭包行全删
        var rootDir = MenuSeedCatalog.AllDefinitions.First(u => u.Type == MenuTypeEnum.Dir
                                                                 && u.ParentKey == null
                                                                 && u.SyncRoles.HasFlag(TenantMenuSyncRole.TenantAdmin)
                                                                 && !u.SuperAdminOnly);
        var rootId = StableSeedId.Menu(rootDir.Key);
        var allIds = MenuSeedCatalog.AllDefinitions.Select(u => StableSeedId.Menu(u.Key)).ToArray();

        await _host.SeedPackageAsync(isEnabled: true, allIds);
        await _host.SeedTenantAsync(TenantMenuSyncTestHost.PackageId);
        await _host.SeedTenantRolesAsync();
        var applier = _host.BuildApplier();
        var tenant = await _host.MainDb.Queryable<SystemTenantEntity>().FirstAsync(u => u.Id == TenantMenuSyncTestHost.TenantId);
        await applier.SyncTenantAsync(tenant, "test:setup");

        var adminRowsBefore = await _host.CountRoleMenusAsync(TenantMenuSyncTestHost.AdminRoleId);
        var customMenuId = (await _host.GetRoleMenuIdsAsync(TenantMenuSyncTestHost.AdminRoleId)).First(u => u != rootId);
        await _host.SeedRoleMenuAsync(TenantMenuSyncTestHost.CustomRoleId, customMenuId, 9001L);

        // 降级：重建套餐清单只含 rootDir
        await _host.MainDb.Deleteable<Entity.Global.GlobalPackageMenuEntity>().ExecuteCommandAsync();
        await _host.MainDb.Insertable(new Entity.Global.GlobalPackageMenuEntity { PackageId = TenantMenuSyncTestHost.PackageId, MenuId = rootId }).ExecuteCommandAsync();

        var downgraded = await applier.SyncTenantAsync(tenant, "test:downgrade");

        Assert.True(downgraded.Succeeded);
        Assert.True(downgraded.DeleteCount > 0);
        // 闭包只含 rootDir 子树（rootDir 无子页面则恰一行）
        var adminAfter = await _host.GetRoleMenuIdsAsync(TenantMenuSyncTestHost.AdminRoleId);
        Assert.All(adminAfter, id => Assert.Contains(id, SubtreeOf(rootDir.Key)));
        // 自定义角色夹逼后为空
        Assert.Empty(await _host.GetRoleMenuIdsAsync(TenantMenuSyncTestHost.CustomRoleId));
        Assert.True(adminRowsBefore > adminAfter.Count);
    }

    [Fact]
    public async Task NullPackage_PureC189Convergence_CustomRoleUntouched()
    {
        await _host.SeedTenantAsync(packageId: null);
        await _host.SeedTenantRolesAsync();
        // 自定义角色持有一行种子菜单：null 套餐下不夹逼 ⇒ 保留
        var someMenuId = StableSeedId.Menu(MenuSeedCatalog.AllDefinitions.First(u => u.Type == MenuTypeEnum.Menu && !u.SuperAdminOnly).Key);
        await _host.SeedRoleMenuAsync(TenantMenuSyncTestHost.CustomRoleId, someMenuId, 9101L);
        // 模板角色持有一行越权种子菜单：SAO 子树内 SyncRoles=None 的按钮（种子域内、不在 admin 期望集），纯 C189 收敛删除
        var surplusDef = MenuSeedCatalog.AllDefinitions.First(u => u.Type == MenuTypeEnum.Button && u.SuperAdminOnly && !u.SyncRoles.HasFlag(TenantMenuSyncRole.TenantAdmin));
        var surplusMenuId = StableSeedId.Menu(surplusDef.Key);
        await _host.SeedRoleMenuAsync(TenantMenuSyncTestHost.AdminRoleId, surplusMenuId, 9102L);

        var applier = _host.BuildApplier();
        var tenant = await _host.MainDb.Queryable<SystemTenantEntity>().FirstAsync(u => u.Id == TenantMenuSyncTestHost.TenantId);
        var result = await applier.SyncTenantAsync(tenant, "test:null");

        Assert.True(result.Succeeded);
        Assert.Contains(someMenuId, await _host.GetRoleMenuIdsAsync(TenantMenuSyncTestHost.CustomRoleId));
        Assert.DoesNotContain(surplusMenuId, await _host.GetRoleMenuIdsAsync(TenantMenuSyncTestHost.AdminRoleId));
        // 模板角色补入完整期望集（含 tenant-readable SAO 辅助按钮）
        var expectedAdmin = MenuSeedCatalog.GetSyncedMenuIds(MenuSeedCatalog.AllDefinitions, TenantMenuSyncRole.TenantAdmin);
        Assert.Equal(expectedAdmin.Count, (await _host.GetRoleMenuIdsAsync(TenantMenuSyncTestHost.AdminRoleId)).Count);
    }

    [Fact]
    public async Task DanglingPackage_SkipAndMarkFailed_NoDataChange()
    {
        // 套餐软删 ⇒ 悬空：不删不补，状态 failed + 专属错误，旧版本不推进
        var allIds = MenuSeedCatalog.AllDefinitions.Select(u => StableSeedId.Menu(u.Key)).ToArray();
        await _host.SeedPackageAsync(isEnabled: true, allIds);
        await _host.SeedTenantAsync(TenantMenuSyncTestHost.PackageId);
        await _host.SeedTenantRolesAsync();
        var applier = _host.BuildApplier();
        var tenant = await _host.MainDb.Queryable<SystemTenantEntity>().FirstAsync(u => u.Id == TenantMenuSyncTestHost.TenantId);
        await applier.SyncTenantAsync(tenant, "test:setup");

        await _host.MainDb.Deleteable<Entity.Global.GlobalPackageEntity>().Where(u => u.Id == TenantMenuSyncTestHost.PackageId).ExecuteCommandAsync();
        var rowsBefore = await _host.GetRoleMenuIdsAsync(TenantMenuSyncTestHost.AdminRoleId);

        var dangling = await applier.SyncTenantAsync(tenant, "test:dangling");

        Assert.False(dangling.Succeeded);
        Assert.True(dangling.SkippedDangling);
        Assert.Equal(rowsBefore.OrderBy(u => u).ToArray(), (await _host.GetRoleMenuIdsAsync(TenantMenuSyncTestHost.AdminRoleId)).OrderBy(u => u).ToArray());

        var state = await _host.GetStateAsync();
        Assert.NotNull(state);
        Assert.Equal(TenantMenuSyncStatusEnum.Failed, state!.Status);
        Assert.StartsWith(TenantMenuSyncApplier.DanglingPackageErrorPrefix, state.LastError ?? string.Empty);
    }

    [Fact]
    public async Task EmptyPackageMenuList_FullConvergenceDeletion()
    {
        // 套餐存在而清单为空 ⇒ 空 ceiling：种子域授权全量收敛删除（有意语义，非悬空）
        var allIds = MenuSeedCatalog.AllDefinitions.Select(u => StableSeedId.Menu(u.Key)).ToArray();
        await _host.SeedPackageAsync(isEnabled: true, allIds);
        await _host.SeedTenantAsync(TenantMenuSyncTestHost.PackageId);
        await _host.SeedTenantRolesAsync();
        var applier = _host.BuildApplier();
        var tenant = await _host.MainDb.Queryable<SystemTenantEntity>().FirstAsync(u => u.Id == TenantMenuSyncTestHost.TenantId);
        await applier.SyncTenantAsync(tenant, "test:setup");
        Assert.True(await _host.CountRoleMenusAsync(TenantMenuSyncTestHost.AdminRoleId) > 0);

        await _host.MainDb.Deleteable<Entity.Global.GlobalPackageMenuEntity>().ExecuteCommandAsync();
        var emptied = await applier.SyncTenantAsync(tenant, "test:empty");

        Assert.True(emptied.Succeeded);
        Assert.True(emptied.DeleteCount > 0);
        Assert.Equal(0, await _host.CountRoleMenusAsync(TenantMenuSyncTestHost.AdminRoleId));
        Assert.Equal(TenantMenuSyncStatusEnum.Succeeded, (await _host.GetStateAsync())!.Status);
    }

    private static long[] SubtreeOf(string rootKey)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal) { rootKey, };
        bool grew = true;
        while (grew)
        {
            grew = false;
            foreach (var definition in MenuSeedCatalog.AllDefinitions)
            {
                if (definition.ParentKey != null && keys.Contains(definition.ParentKey) && keys.Add(definition.Key))
                {
                    grew = true;
                }
            }
        }

        return keys.Select(StableSeedId.Menu).ToArray();
    }

    public void Dispose()
    {
        _host.Dispose();
    }
}
