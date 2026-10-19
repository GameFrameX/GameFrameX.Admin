using GameFrameX.Core.Const;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// C189：menu seed SyncRoles 变更后 role-menu 幂等重同步的单测。
/// 验证 <see cref="MenuSeedCatalog.GetSurplusSeedMenuIds"/> 在 SyncRoles 增/减/改三种变更下的收敛行为，
/// 以及"补入(BuildRoleMenus) + 删除(surplus)"基于同一期望集的互补性。
/// </summary>
[Collection("MenuSeedCatalog")]
public class RoleMenuResyncTests
{
    /// <summary>
    /// 基线：显式 SyncRoles 授权的菜单（及其 TenantAdmin 父回退按钮）不在 surplus；
    /// 未授权的种子菜单在 surplus。surplus 只含种子菜单范围。
    /// </summary>
    [Fact]
    public void Surplus_IncludesUnsyncedSeedMenus_ExcludesSynced()
    {
        var definitions = SampleTree(TenantMenuSyncRole.TenantAdmin, TenantMenuSyncRole.None);
        var surplus = MenuSeedCatalog.GetSurplusSeedMenuIds(definitions, TenantMenuSyncRole.TenantAdmin);

        // 期望集（不删）：dirA、pageA、pageA:page（TenantAdmin 父回退按钮）
        Assert.DoesNotContain(StableSeedId.Menu("route:dirA"), surplus);
        Assert.DoesNotContain(StableSeedId.Menu("route:pageA"), surplus);
        Assert.DoesNotContain(StableSeedId.Menu("pageA:page"), surplus);

        // surplus（应删）：dirB、pageB、pageB:page
        Assert.Contains(StableSeedId.Menu("route:dirB"), surplus);
        Assert.Contains(StableSeedId.Menu("route:pageB"), surplus);
        Assert.Contains(StableSeedId.Menu("pageB:page"), surplus);
        Assert.Equal(3, surplus.Count);
    }

    /// <summary>
    /// 减少方向：把已授权菜单 pageA 的 SyncRoles 从 TenantAdmin 改为 None，
    /// pageA 及其父回退按钮 pageA:page 都应进入 surplus（父不再同步则按钮不再回退）。
    /// </summary>
    [Fact]
    public void Surplus_Grows_WhenSyncRolesReduced()
    {
        var definitions = SampleTree(TenantMenuSyncRole.None, TenantMenuSyncRole.None);
        var surplus = MenuSeedCatalog.GetSurplusSeedMenuIds(definitions, TenantMenuSyncRole.TenantAdmin);

        Assert.Contains(StableSeedId.Menu("route:pageA"), surplus);
        Assert.Contains(StableSeedId.Menu("pageA:page"), surplus);
        // dirA 仍授权（SyncRoles=TenantAdmin），不在 surplus
        Assert.DoesNotContain(StableSeedId.Menu("route:dirA"), surplus);
    }

    /// <summary>
    /// 增加方向：把未授权菜单 dirB 的 SyncRoles 从 None 改为 TenantAdmin，
    /// dirA 应移出 surplus（被纳入期望集）。
    /// </summary>
    [Fact]
    public void Surplus_Shrinks_WhenSyncRolesGranted()
    {
        var definitions = SampleTree(TenantMenuSyncRole.TenantAdmin, TenantMenuSyncRole.TenantAdmin);
        var surplus = MenuSeedCatalog.GetSurplusSeedMenuIds(definitions, TenantMenuSyncRole.TenantAdmin);

        // dirB 现已授权，移出 surplus；pageB/pageB:page 仍未授权（pageB SyncRoles=None，按钮无父回退）
        Assert.DoesNotContain(StableSeedId.Menu("route:dirB"), surplus);
        Assert.Contains(StableSeedId.Menu("route:pageB"), surplus);
        Assert.Contains(StableSeedId.Menu("pageB:page"), surplus);
    }

    /// <summary>
    /// 幂等：同一 SyncRoles 配置多次计算，surplus 结果一致。
    /// </summary>
    [Fact]
    public void Surplus_IsIdempotent_AcrossRuns()
    {
        var definitions = SampleTree(TenantMenuSyncRole.TenantAdmin, TenantMenuSyncRole.None);

        var first = MenuSeedCatalog.GetSurplusSeedMenuIds(definitions, TenantMenuSyncRole.TenantAdmin)
                                   .OrderBy(u => u)
                                   .ToArray();
        var second = MenuSeedCatalog.GetSurplusSeedMenuIds(definitions, TenantMenuSyncRole.TenantAdmin)
                                    .OrderBy(u => u)
                                    .ToArray();

        Assert.Equal(first, second);
    }

    /// <summary>
    /// 未知角色编码（解析为 None）不应产生任何删除候选，避免误删。
    /// </summary>
    [Fact]
    public void Surplus_ReturnsEmpty_WhenRoleIsNone()
    {
        var definitions = SampleTree(TenantMenuSyncRole.TenantAdmin, TenantMenuSyncRole.None);
        var surplus = MenuSeedCatalog.GetSurplusSeedMenuIds(definitions, TenantMenuSyncRole.None);

        Assert.Empty(surplus);
    }

    /// <summary>
    /// 互补性（真实 seed）：BuildRoleMenus 的期望集与 surplus 不重叠，且并集覆盖全部种子菜单 Id。
    /// 这保证"补入"与"删除"基于完全一致的期望集，不会同时漏补与误删。
    /// </summary>
    [Fact]
    public void Surplus_IsComplementaryToBuildRoleMenus_OnRealSeed()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();
        var adminRole = TenantDefaultDataTemplates.RoleTemplates.Single(u => u.Code == TenantDefaultDataTemplates.AdminRoleCode);

        var expected = MenuSeedCatalog.BuildRoleMenus(adminRole.SeedId, adminRole.Code)
                                      .Select(u => u.MenuId)
                                      .ToHashSet();
        var surplus = MenuSeedCatalog.GetSurplusSeedMenuIds(adminRole.Code).ToHashSet();
        var allSeed = MenuSeedCatalog.AllDefinitions
                                     .Select(u => StableSeedId.Menu(u.Key))
                                     .ToHashSet();

        // 期望与 surplus 不重叠
        Assert.Empty(expected.Intersect(surplus));
        // 两者并集覆盖全部种子菜单 Id
        Assert.Equal(allSeed.Count, expected.Count + surplus.Count);
        // surplus 全部属于种子菜单范围
        Assert.True(surplus.IsSubsetOf(allSeed));
    }

    /// <summary>
    /// 验收对照（C188 cda9e60f）：tenantChannelDefinition 列表页菜单及其 :page/:detail/:add/:delete/:update
    /// 权限按钮已在 TenantAdmin 期望集中，不应出现在 surplus（重同步不会误删 → API 不再 403）。
    /// </summary>
    [Fact]
    public void TenantChannelDefinition_NotInTenantAdminSurplus()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();
        var surplus = MenuSeedCatalog.GetSurplusSeedMenuIds(TenantDefaultDataTemplates.AdminRoleCode).ToHashSet();

        var expectedKeys = new[]
        {
            "route:tenantChannelDefinition",
            "tenantChannelDefinition:page",
            "tenantChannelDefinition:detail",
            "tenantChannelDefinition:add",
            "tenantChannelDefinition:delete",
            "tenantChannelDefinition:update",
        };

        foreach (var key in expectedKeys)
        {
            Assert.DoesNotContain(StableSeedId.Menu(key), surplus);
        }
    }

    private static MenuSeedDefinition Def(string key, string? parent, MenuTypeEnum type, TenantMenuSyncRole sync, string? permission)
    {
        return new MenuSeedDefinition(
            key,
            parent,
            type,
            key,
            null,
            null,
            null,
            permission,
            key,
            null,
            null,
            null,
            false,
            false,
            true,
            false,
            0,
            true,
            null,
            0L,
            0L,
            sync,
            false,
            false,
            MenuAppScopeEnum.App);
    }

    private static List<MenuSeedDefinition> SampleTree(TenantMenuSyncRole pageASync, TenantMenuSyncRole dirBSync)
    {
        return new List<MenuSeedDefinition>
        {
            Def("route:dirA", null, MenuTypeEnum.Dir, TenantMenuSyncRole.TenantAdmin, null),
            Def("route:pageA", "route:dirA", MenuTypeEnum.Menu, pageASync, null),
            Def("pageA:page", "route:pageA", MenuTypeEnum.Button, TenantMenuSyncRole.None, "pageA:page"),
            Def("route:dirB", null, MenuTypeEnum.Dir, dirBSync, null),
            Def("route:pageB", "route:dirB", MenuTypeEnum.Menu, TenantMenuSyncRole.None, null),
            Def("pageB:page", "route:pageB", MenuTypeEnum.Button, TenantMenuSyncRole.None, "pageB:page"),
        };
    }
}