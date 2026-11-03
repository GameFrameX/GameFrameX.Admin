using GameFrameX.Core.Const;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.Service.TenantMenuSync;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.System;

namespace GameFrameX.Core.Tests.TenantMenuSync;

/// <summary>
/// C354：套餐语义激活与租户生命周期对齐。
/// <para>T1/T7 新租户初始授权 = 全量默认授权 ∩ 套餐闭包（CreateTenantAdminRoleMenus / IntersectDefaultAdminMenuIds）；
/// T2 Enterprise 套餐菜单启动幂等补全（收敛 / 幂等 / 手工损伤自愈 / 种子增长跟随）；
/// T3 套餐保存祖先闭包补全（补祖先 / 不越 SAO 边界 / 非种子保留 / 幂等有序）；
/// T5 套餐分配校验矩阵（可用性 / 默认租户禁改）；T6 Enterprise ceiling ∪ tenant-readable 无振荡守护
/// （C189 全量基线是不动点——同步零删除、二轮零变更；默认租户纳入同步范围）。</para>
/// </summary>
[Collection("MenuSeedCatalog")]
public class TenantPackageCeilingAlignmentTests : IDisposable
{
    private const string RootKey = "route:root";
    private const string PageKey = "route:page";
    private const string ButtonKey = "page:btn";
    private const string SaoRootKey = "route:sao";
    private const string SaoPageKey = "route:saoPage";
    private const string SaoButtonKey = "saoPage:btn";
    private const string TenantReadableButtonKey = "saoPage:trBtn";

    private const int TenantPackageNotUsableCode = 30600014;
    private const int DefaultTenantPackageChangeForbiddenCode = 30600015;

    private readonly TenantMenuSyncTestHost _host = new();

    // ---------- T3：CompleteAncestors 纯函数 ----------

    private static MenuSeedDefinition Def(string key, string? parentKey, MenuTypeEnum type, TenantMenuSyncRole roles, bool superAdminOnly = false, bool allowTenant = false)
    {
        return new MenuSeedDefinition(
            key, parentKey, type, key, key, key, null, null, key, null, null, null,
            false, false, true, false, 100, true, null, 0, 0, roles, superAdminOnly, allowTenant, MenuAppScopeEnum.App);
    }

    /// <summary>
    /// 两棵树：root→page→btn；saoRoot（SuperAdmin-only）→ saoPage → saoBtn + tenant-readable 辅助按钮。
    /// </summary>
    private static List<MenuSeedDefinition> BuildDefinitions()
    {
        return new List<MenuSeedDefinition>
        {
            Def(RootKey, null, MenuTypeEnum.Dir, TenantMenuSyncRole.TenantAdmin),
            Def(PageKey, RootKey, MenuTypeEnum.Menu, TenantMenuSyncRole.TenantAdmin),
            Def(ButtonKey, PageKey, MenuTypeEnum.Button, TenantMenuSyncRole.TenantAdmin),
            Def(SaoRootKey, null, MenuTypeEnum.Dir, TenantMenuSyncRole.None, superAdminOnly: true),
            Def(SaoPageKey, SaoRootKey, MenuTypeEnum.Menu, TenantMenuSyncRole.None, superAdminOnly: true),
            Def(SaoButtonKey, SaoPageKey, MenuTypeEnum.Button, TenantMenuSyncRole.None, superAdminOnly: true),
            Def(TenantReadableButtonKey, SaoPageKey, MenuTypeEnum.Button, TenantMenuSyncRole.TenantAdmin, superAdminOnly: true, allowTenant: true),
        };
    }

    [Fact]
    public void CompleteAncestors_PageOnly_PullsAncestorDirectory_WithoutDescendantButtons()
    {
        var closure = TenantPackageMenuClosure.CompleteAncestors(new[] { StableSeedId.Menu(PageKey) }, BuildDefinitions());

        Assert.Equal(new[] { StableSeedId.Menu(RootKey), StableSeedId.Menu(PageKey) }.OrderBy(u => u), closure);
    }

    [Fact]
    public void CompleteAncestors_FromButton_PullsFullParentChain()
    {
        var closure = TenantPackageMenuClosure.CompleteAncestors(new[] { StableSeedId.Menu(ButtonKey) }, BuildDefinitions());

        Assert.Contains(StableSeedId.Menu(PageKey), closure);
        Assert.Contains(StableSeedId.Menu(RootKey), closure);
    }

    [Fact]
    public void CompleteAncestors_TenantReadableSaoButton_DoesNotCrossSaoBoundary()
    {
        // tenant-readable 按钮父链是超管目录，补祖先不得把 SAO 菜单带回清单
        var closure = TenantPackageMenuClosure.CompleteAncestors(new[] { StableSeedId.Menu(TenantReadableButtonKey) }, BuildDefinitions());

        Assert.Equal(new[] { StableSeedId.Menu(TenantReadableButtonKey) }, closure);
    }

    [Fact]
    public void CompleteAncestors_PreservesNonSeedIds_IdempotentAndSorted()
    {
        var definitions = BuildDefinitions();
        var closure = TenantPackageMenuClosure.CompleteAncestors(new[] { 999999L, StableSeedId.Menu(ButtonKey) }, definitions);

        Assert.Contains(999999L, closure);
        Assert.Equal(closure.OrderBy(u => u), closure);

        var twice = TenantPackageMenuClosure.CompleteAncestors(closure.ToArray(), definitions);
        Assert.Equal(closure, twice);
    }

    [Fact]
    public void EnterprisePackageMenuIds_IsNonSaoSeedDomainPlusTenantReadableButtons()
    {
        var ids = TenantPackageMenuClosure.EnterprisePackageMenuIds(BuildDefinitions());

        Assert.Equal(new[]
        {
            StableSeedId.Menu(RootKey),
            StableSeedId.Menu(PageKey),
            StableSeedId.Menu(ButtonKey),
            StableSeedId.Menu(TenantReadableButtonKey),
        }.OrderBy(u => u), ids);
    }

    // ---------- T2：Enterprise 套餐菜单启动幂等补全 ----------

    [Fact]
    public async Task EnsureCompleted_ConvergesEnterpriseRows_IdempotentAndSelfHealing()
    {
        await SeedEnterprisePackageAsync();
        var canonical = TenantPackageMenuClosure.EnterprisePackageMenuIds(MenuSeedCatalog.AllDefinitions);

        EnterprisePackageMenuCompletion.EnsureCompleted(_host.MainDb);
        Assert.Equal(canonical, await GetEnterpriseMenuIdsAsync());

        // 幂等：重复执行零变化
        EnterprisePackageMenuCompletion.EnsureCompleted(_host.MainDb);
        Assert.Equal(canonical, await GetEnterpriseMenuIdsAsync());

        // 手工损伤（删一行 + 塞外来行）→ 启动恢复
        await _host.MainDb.Deleteable<GlobalPackageMenuEntity>()
                    .Where(u => u.PackageId == TenantDefaultDataTemplates.EnterprisePackageId && u.MenuId == canonical[0])
                    .ExecuteCommandAsync();
        await _host.MainDb.Insertable(new GlobalPackageMenuEntity { Id = 0, PackageId = TenantDefaultDataTemplates.EnterprisePackageId, MenuId = 999999L }).ExecuteCommandAsync();
        EnterprisePackageMenuCompletion.EnsureCompleted(_host.MainDb);
        Assert.Equal(canonical, await GetEnterpriseMenuIdsAsync());

        // 旧版本更小清单（种子演化增长）→ 启动收敛到当前全集
        await _host.MainDb.Deleteable<GlobalPackageMenuEntity>()
                    .Where(u => u.PackageId == TenantDefaultDataTemplates.EnterprisePackageId)
                    .ExecuteCommandAsync();
        await _host.MainDb.Insertable(new GlobalPackageMenuEntity { Id = 0, PackageId = TenantDefaultDataTemplates.EnterprisePackageId, MenuId = canonical[^1] }).ExecuteCommandAsync();
        EnterprisePackageMenuCompletion.EnsureCompleted(_host.MainDb);
        Assert.Equal(canonical, await GetEnterpriseMenuIdsAsync());
    }

    // ---------- T6：Enterprise ceiling 无振荡守护（默认租户纳入同步范围） ----------

    [Fact]
    public async Task EnterpriseCeiling_UnionsTenantReadableButtons_SupersetOfAllTemplateRoleExpectations()
    {
        await SeedEnterprisePackageAsync();
        EnterprisePackageMenuCompletion.EnsureCompleted(_host.MainDb);
        await _host.SeedTenantAsync(TenantDefaultDataTemplates.EnterprisePackageId);

        var tenant = await _host.MainDb.Queryable<SystemTenantEntity>().FirstAsync(u => u.Id == TenantMenuSyncTestHost.TenantId);
        var ceiling = await TenantMenuSyncApplier.ComputeCeilingAsync(_host.Client, tenant);

        Assert.Equal(TenantMenuSyncCeilingKind.Bounded, ceiling.Kind);

        // 无振荡不变式：ceiling ⊇ 全部模板角色 SyncRoles 期望集 ⇒ 引擎夹取为无操作，默认租户收敛结果 = C189 全量
        var syncRoles = TenantDefaultDataTemplates.RoleTemplates
                                                  .Select(u => u.SyncRole)
                                                  .Where(u => u != TenantMenuSyncRole.None)
                                                  .Distinct()
                                                  .ToList();
        var allRoleExpectedIds = new HashSet<long>();
        foreach (var role in syncRoles)
        {
            var expected = MenuSeedCatalog.GetSyncedMenuIds(MenuSeedCatalog.AllDefinitions, role);
            allRoleExpectedIds.UnionWith(expected);
            Assert.All(expected, id => Assert.True(ceiling.MenuIds.Contains(id), $"SyncRole={role} 期望菜单 {id} 不在 Enterprise ceiling 内"));
        }

        // union 生效：tenant-readable SAO 辅助按钮（globalPlatform:Select / globalLanguage:Select）在期望集内 → 在 ceiling 内
        Assert.All(MenuSeedCatalog.TenantReadableSuperAdminOnlyMenuIds, id => Assert.True(ceiling.MenuIds.Contains(id)));

        // 安全边界：无任何模板角色期望的纯 SAO 菜单（超管专属页面等）仍被排除
        var saoBeyondExpectations = MenuSeedCatalog.SuperAdminOnlySubtreeMenuIds
                                                    .Where(u => !allRoleExpectedIds.Contains(u))
                                                    .ToList();
        Assert.NotEmpty(saoBeyondExpectations);
        Assert.All(saoBeyondExpectations, id => Assert.False(ceiling.MenuIds.Contains(id)));
    }

    [Fact]
    public async Task DefaultTenantConvergence_C189FullSetIsFixedPoint_NoOscillation()
    {
        await SeedEnterprisePackageAsync();
        EnterprisePackageMenuCompletion.EnsureCompleted(_host.MainDb);
        await _host.SeedTenantAsync(TenantDefaultDataTemplates.EnterprisePackageId);
        await _host.SeedTenantRolesAsync();

        // C189 seeder 等价基线：sys_admin 持全量默认授权（含 tenant-readable SAO 辅助按钮）
        var rowId = 9100L;
        foreach (var menuId in TenantAdminMenuDefaults.MenuIds)
        {
            await _host.SeedRoleMenuAsync(TenantMenuSyncTestHost.AdminRoleId, menuId, rowId);
            rowId++;
        }

        var applier = _host.BuildApplier();
        var tenant = await _host.MainDb.Queryable<SystemTenantEntity>().FirstAsync(u => u.Id == TenantMenuSyncTestHost.TenantId);

        var first = await applier.SyncTenantAsync(tenant, "test:c354-default-tenant");
        Assert.True(first.Succeeded);
        // 无振荡核心断言：C189 全量基线不被判 surplus——零删除（无 union 时 tenant-readable 按钮会被删、重启又被 seeder 补回）
        Assert.Equal(0, first.DeleteCount);

        var expectedAdmin = MenuSeedCatalog.GetSyncedMenuIds(MenuSeedCatalog.AllDefinitions, TenantMenuSyncRole.TenantAdmin).OrderBy(u => u).ToArray();
        var actualAdmin = (await _host.GetRoleMenuIdsAsync(TenantMenuSyncTestHost.AdminRoleId)).OrderBy(u => u).ToArray();
        Assert.Equal(expectedAdmin, actualAdmin);

        // 第二轮 = 不动点（同步自身产出不再引发变更）
        var second = await applier.SyncTenantAsync(tenant, "test:c354-idempotent");
        Assert.True(second.Succeeded);
        Assert.Equal(0, second.InsertCount);
        Assert.Equal(0, second.DeleteCount);
    }

    // ---------- 辅助 ----------

    private async Task SeedEnterprisePackageAsync()
    {
        await _host.MainDb.Insertable(new GlobalPackageEntity
        {
            Id = TenantDefaultDataTemplates.EnterprisePackageId,
            Name = "企业版",
            Code = "enterprise",
            IsEnabled = true,
        }).ExecuteCommandAsync();
    }

    private async Task<List<long>> GetEnterpriseMenuIdsAsync()
    {
        return await _host.MainDb.Queryable<GlobalPackageMenuEntity>()
                          .Where(u => u.PackageId == TenantDefaultDataTemplates.EnterprisePackageId)
                          .OrderBy(u => u.MenuId)
                          .Select(u => u.MenuId)
                          .ToListAsync();
    }

    public void Dispose()
    {
        _host.Dispose();
    }
}
