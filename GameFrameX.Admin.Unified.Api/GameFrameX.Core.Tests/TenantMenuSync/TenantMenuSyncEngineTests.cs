using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.Service.TenantMenuSync;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Tests.TenantMenuSync;

/// <summary>
/// C353 T3：同步引擎纯函数 diff 矩阵。三层权责 + 幂等保留 + 防误删语义全锁定。
/// </summary>
public class TenantMenuSyncEngineTests
{
    private const long TenantId = 2200000000001;
    private const long AdminRoleId = 1001;
    private const long OpsRoleId = 1002;
    private const long CustomRoleId = 2001;
    private const string AdminCode = "sys_admin";
    private const string OpsCode = "ops_manager";
    private const string CustomCode = "my_custom";

    private static MenuSeedDefinition Def(string key, string? parentKey, MenuTypeEnum type, TenantMenuSyncRole roles, bool superAdminOnly = false, bool allowTenantWhenSao = false)
    {
        return new MenuSeedDefinition(
            key, parentKey, type, key, key, key, null, null, key, null, null, null,
            false, false, true, false, 100, true, null, 0, 0, roles, superAdminOnly, allowTenantWhenSao, MenuAppScopeEnum.App);
    }

    /// <summary>
    /// root(dir) → page(menu, Admin|Ops) → btn(button, Admin)；opsOnly(menu, Ops)；customOnly(menu, None=仅自定义角色用)；
    /// saoRoot(SAO 根, tenant-readable 辅助按钮 allowBtn 挂其下)。
    /// </summary>
    private static List<MenuSeedDefinition> BuildDefinitions()
    {
        return new List<MenuSeedDefinition>
        {
            Def("route:root", null, MenuTypeEnum.Dir, TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.OpsManager),
            Def("route:page", "route:root", MenuTypeEnum.Menu, TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.OpsManager),
            Def("page:btn", "route:page", MenuTypeEnum.Button, TenantMenuSyncRole.TenantAdmin),
            Def("route:opsOnly", "route:root", MenuTypeEnum.Menu, TenantMenuSyncRole.OpsManager),
            Def("route:customOnly", "route:root", MenuTypeEnum.Menu, TenantMenuSyncRole.None),
            Def("route:sao", null, MenuTypeEnum.Dir, TenantMenuSyncRole.None, superAdminOnly: true),
            Def("route:saoHelper", "route:sao", MenuTypeEnum.Button, TenantMenuSyncRole.TenantAdmin, superAdminOnly: true, allowTenantWhenSao: true),
        };
    }

    private static List<TenantMenuSyncEngine.RoleTemplateSnapshot> BuildTemplates()
    {
        return new List<TenantMenuSyncEngine.RoleTemplateSnapshot>
        {
            new(AdminCode, TenantMenuSyncRole.TenantAdmin),
            new(OpsCode, TenantMenuSyncRole.OpsManager),
        };
    }

    private static TenantMenuSyncEngine.Input BuildInput(
        TenantMenuSyncPackageCeiling ceiling,
        List<TenantMenuSyncEngine.RoleSnapshot>? roles = null,
        List<TenantMenuSyncEngine.RoleMenuRowSnapshot>? roleMenus = null)
    {
        return new TenantMenuSyncEngine.Input(
            TenantId,
            ceiling,
            BuildDefinitions(),
            BuildTemplates(),
            roles ?? new List<TenantMenuSyncEngine.RoleSnapshot>
            {
                new(AdminRoleId, AdminCode),
                new(OpsRoleId, OpsCode),
                new(CustomRoleId, CustomCode),
            },
            roleMenus ?? new List<TenantMenuSyncEngine.RoleMenuRowSnapshot>());
    }

    private static TenantMenuSyncEngine.Diff Compute(
        TenantMenuSyncPackageCeiling ceiling,
        List<TenantMenuSyncEngine.RoleSnapshot>? roles = null,
        List<TenantMenuSyncEngine.RoleMenuRowSnapshot>? roleMenus = null)
    {
        return TenantMenuSyncEngine.Compute(BuildInput(ceiling, roles, roleMenus));
    }

    [Fact]
    public void NoCeiling_TemplateRolesConvergeToPureC189Baseline()
    {
        var diff = Compute(TenantMenuSyncPackageCeiling.NoCeiling);

        // sys_admin 期望：root/page/page:btn/customOnly 之外 —— customOnly 是 None 不同步；
        // sao 子树按 C189 语义 SelectSyncedDefinitions 决定（tenant-readable 辅助按钮属于期望集）
        Assert.Contains(diff.Inserts, u => u.RoleId == AdminRoleId && u.MenuId == StableSeedId.Menu("route:root"));
        Assert.Contains(diff.Inserts, u => u.RoleId == AdminRoleId && u.MenuId == StableSeedId.Menu("route:page"));
        Assert.Contains(diff.Inserts, u => u.RoleId == AdminRoleId && u.MenuId == StableSeedId.Menu("page:btn"));
        Assert.DoesNotContain(diff.Inserts, u => u.RoleId == AdminRoleId && u.MenuId == StableSeedId.Menu("route:customOnly"));
        // ops_manager 期望 opsOnly
        Assert.Contains(diff.Inserts, u => u.RoleId == OpsRoleId && u.MenuId == StableSeedId.Menu("route:opsOnly"));
        // 自定义角色不补入
        Assert.DoesNotContain(diff.Inserts, u => u.RoleId == CustomRoleId);
        Assert.Empty(diff.DeleteRowIds);
        // 确定性主键
        var insert = diff.Inserts.Single(u => u.RoleId == AdminRoleId && u.MenuId == StableSeedId.Menu("route:page"));
        Assert.Equal(StableSeedId.RoleMenu(AdminCode, "route:page"), insert.Id);
    }

    [Fact]
    public void NoCeiling_TemplateRolesKeepTenantReadableSaoHelperButtons()
    {
        // SAO 防御只在闭包层；引擎期望集保持纯 C189 语义 —— tenant-readable 辅助按钮
        // （SyncRoles 命中，ShouldSyncToRole 第一分支）必入期望集且 null 套餐下不删不漏
        var diff = Compute(TenantMenuSyncPackageCeiling.NoCeiling);

        Assert.Contains(diff.Inserts, u => u.RoleId == AdminRoleId && u.MenuId == StableSeedId.Menu("route:saoHelper"));
        // SAO 根目录（SyncRoles=None）不进任何期望集
        Assert.DoesNotContain(diff.Inserts, u => u.MenuId == StableSeedId.Menu("route:sao"));
    }

    [Fact]
    public void BoundedCeilingWithoutSao_SaoHelperRowsDeletedFromTemplateRoles()
    {
        // ceiling 不含 SAO 子树（闭包层已防御剔除）⇒ 模板角色已有的辅助按钮行按夹逼删除
        var ceiling = TenantMenuSyncPackageCeiling.Bounded(new HashSet<long>
        {
            StableSeedId.Menu("route:root"),
            StableSeedId.Menu("route:page"),
        });
        var saoHelperRow = 6666L;
        var roleMenus = new List<TenantMenuSyncEngine.RoleMenuRowSnapshot>
        {
            new(saoHelperRow, AdminRoleId, StableSeedId.Menu("route:saoHelper")),
        };

        var diff = Compute(ceiling, roleMenus: roleMenus);

        Assert.DoesNotContain(diff.Inserts, u => u.MenuId == StableSeedId.Menu("route:saoHelper"));
        Assert.Contains(diff.DeleteRowIds, id => id == saoHelperRow);
    }

    [Fact]
    public void ExistingExactBaseline_IdempotentZeroDiff()
    {
        var existing = new List<TenantMenuSyncEngine.RoleMenuRowSnapshot>();
        foreach (var menuKey in new[] { "route:root", "route:page", "page:btn", "route:saoHelper" })
        {
            existing.Add(new TenantMenuSyncEngine.RoleMenuRowSnapshot(
                StableSeedId.RoleMenu(AdminCode, menuKey), AdminRoleId, StableSeedId.Menu(menuKey)));
        }

        existing.Add(new TenantMenuSyncEngine.RoleMenuRowSnapshot(
            StableSeedId.RoleMenu(OpsCode, "route:root"), OpsRoleId, StableSeedId.Menu("route:root")));
        existing.Add(new TenantMenuSyncEngine.RoleMenuRowSnapshot(
            StableSeedId.RoleMenu(OpsCode, "route:page"), OpsRoleId, StableSeedId.Menu("route:page")));
        existing.Add(new TenantMenuSyncEngine.RoleMenuRowSnapshot(
            StableSeedId.RoleMenu(OpsCode, "route:opsOnly"), OpsRoleId, StableSeedId.Menu("route:opsOnly")));

        var diff = Compute(TenantMenuSyncPackageCeiling.NoCeiling, roleMenus: existing);

        // customOnly 为 None：sys_admin 不期望、也不删（无此行）；全部期望已就位 ⇒ 零插入零删除
        Assert.Empty(diff.Inserts.Where(u => u.RoleId == AdminRoleId));
        Assert.Empty(diff.DeleteRowIds);
    }

    [Fact]
    public void TemplateSurplusSeedRows_Deleted()
    {
        // sys_admin 持有 opsOnly（超出其基线）⇒ surplus 删除
        var surplusRowId = 5555L;
        var roleMenus = new List<TenantMenuSyncEngine.RoleMenuRowSnapshot>
        {
            new(surplusRowId, AdminRoleId, StableSeedId.Menu("route:opsOnly")),
        };

        var diff = Compute(TenantMenuSyncPackageCeiling.NoCeiling, roleMenus: roleMenus);

        Assert.Contains(diff.DeleteRowIds, id => id == surplusRowId);
    }

    [Fact]
    public void BoundedCeiling_TemplateExpectedClampedByPackage()
    {
        // 套餐闭包只含 root/page/btn：ops 角色的 opsOnly 期望被夹掉（不补入），已有行删除
        var ceiling = TenantMenuSyncPackageCeiling.Bounded(new HashSet<long>
        {
            StableSeedId.Menu("route:root"),
            StableSeedId.Menu("route:page"),
            StableSeedId.Menu("page:btn"),
        });

        var diff = Compute(ceiling);

        Assert.DoesNotContain(diff.Inserts, u => u.MenuId == StableSeedId.Menu("route:opsOnly"));
        Assert.Contains(diff.Inserts, u => u.RoleId == AdminRoleId && u.MenuId == StableSeedId.Menu("route:page"));
        Assert.Contains(diff.Inserts, u => u.RoleId == OpsRoleId && u.MenuId == StableSeedId.Menu("route:page"));
    }

    [Fact]
    public void BoundedCeiling_CustomRoleClampOnly_NoInserts()
    {
        var ceiling = TenantMenuSyncPackageCeiling.Bounded(new HashSet<long>
        {
            StableSeedId.Menu("route:root"),
            StableSeedId.Menu("route:page"),
            StableSeedId.Menu("page:btn"),
        });

        var beyondRowId = 7777L;
        var withinRowId = 7778L;
        var deadRowId = 7779L;
        var roleMenus = new List<TenantMenuSyncEngine.RoleMenuRowSnapshot>
        {
            // 自定义角色持有超 ceiling 种子行（opsOnly）⇒ 删；ceiling 内行保留；死行（非种子域）⇒ 删
            new(beyondRowId, CustomRoleId, StableSeedId.Menu("route:opsOnly")),
            new(withinRowId, CustomRoleId, StableSeedId.Menu("route:page")),
            new(deadRowId, CustomRoleId, 4242424242424L),
        };

        var diff = Compute(ceiling, roleMenus: roleMenus);

        Assert.Contains(diff.DeleteRowIds, id => id == beyondRowId);
        Assert.Contains(diff.DeleteRowIds, id => id == deadRowId);
        Assert.DoesNotContain(diff.DeleteRowIds, id => id == withinRowId);
        Assert.DoesNotContain(diff.Inserts, u => u.RoleId == CustomRoleId);
    }

    [Fact]
    public void NoCeiling_CustomRoleNotClamped()
    {
        // null 套餐：自定义角色不做夹逼（仅死行清理）
        var rowId = 8888L;
        var roleMenus = new List<TenantMenuSyncEngine.RoleMenuRowSnapshot>
        {
            new(rowId, CustomRoleId, StableSeedId.Menu("route:opsOnly")),
        };

        var diff = Compute(TenantMenuSyncPackageCeiling.NoCeiling, roleMenus: roleMenus);

        Assert.DoesNotContain(diff.DeleteRowIds, id => id == rowId);
    }

    [Fact]
    public void DuplicateTemplateCode_WholeGroupTreatedAsCustom_WithWarning()
    {
        // 租户自建同 Code 角色：sys_admin 命中 2 个 ⇒ 整组按自定义角色处理 + 告警；
        // Bounded 下组内行按夹逼处理（ceiling 内保留），绝不按模板基线删除 surplus
        var ceiling = TenantMenuSyncPackageCeiling.Bounded(new HashSet<long>
        {
            StableSeedId.Menu("route:root"),
            StableSeedId.Menu("route:page"),
        });
        var realAdminRow = 3111L;
        var fakeAdminRow = 3112L;
        var roles = new List<TenantMenuSyncEngine.RoleSnapshot>
        {
            new(AdminRoleId, AdminCode),
            new(1003L, AdminCode), // 冒名同 Code
            new(CustomRoleId, CustomCode),
        };
        var roleMenus = new List<TenantMenuSyncEngine.RoleMenuRowSnapshot>
        {
            // 真模板角色持有超出模板基线但在 ceiling 内的行：customOnly 不在任何 ceiling —— 换 opsOnly（ceiling 外）
            new(realAdminRow, AdminRoleId, StableSeedId.Menu("route:opsOnly")),
            new(fakeAdminRow, 1003L, StableSeedId.Menu("route:opsOnly")),
        };

        var diff = Compute(ceiling, roles: roles, roleMenus: roleMenus);

        Assert.Contains(diff.Warnings, w => w.Code == "duplicate-template-code");
        // 两个同 Code 角色的 opsOnly 行都在 ceiling 外 ⇒ 夹逼删除（自定义语义），且无模板补入
        Assert.Contains(diff.DeleteRowIds, id => id == realAdminRow);
        Assert.Contains(diff.DeleteRowIds, id => id == fakeAdminRow);
        Assert.DoesNotContain(diff.Inserts, u => u.RoleId == AdminRoleId || u.RoleId == 1003L);
    }

    [Fact]
    public void NoneSyncRole_ZeroDeleteCandidates_AntiMisdelete()
    {
        // 模板角色 SyncRole == None：零删除候选（防误删），死行清理仍生效
        var noneCode = "auditor_none";
        var noneRoleId = 4001L;
        var templates = new List<TenantMenuSyncEngine.RoleTemplateSnapshot>
        {
            new(AdminCode, TenantMenuSyncRole.TenantAdmin),
            new(noneCode, TenantMenuSyncRole.None),
        };
        var surplusRow = 4111L;
        var deadRow = 4112L;
        var roles = new List<TenantMenuSyncEngine.RoleSnapshot>
        {
            new(AdminRoleId, AdminCode),
            new(noneRoleId, noneCode),
        };
        var roleMenus = new List<TenantMenuSyncEngine.RoleMenuRowSnapshot>
        {
            // None 角色持有 sys_admin 基线菜单（越权行）—— 按安全语义不删
            new(surplusRow, noneRoleId, StableSeedId.Menu("route:page")),
            new(deadRow, noneRoleId, 9999999999999L),
        };

        var input = new TenantMenuSyncEngine.Input(TenantId, TenantMenuSyncPackageCeiling.NoCeiling, BuildDefinitions(), templates, roles, roleMenus);
        var diff = TenantMenuSyncEngine.Compute(input);

        Assert.DoesNotContain(diff.DeleteRowIds, id => id == surplusRow);
        Assert.Contains(diff.DeleteRowIds, id => id == deadRow);
        Assert.DoesNotContain(diff.Inserts, u => u.RoleId == noneRoleId);
    }

    [Fact]
    public void BusinessKeyDuplicateRows_KeepMinId_DeleteOthers()
    {
        var keepId = 100L;
        var dupId = 200L;
        var roleMenus = new List<TenantMenuSyncEngine.RoleMenuRowSnapshot>
        {
            new(keepId, AdminRoleId, StableSeedId.Menu("route:page")),
            new(dupId, AdminRoleId, StableSeedId.Menu("route:page")),
        };

        var diff = Compute(TenantMenuSyncPackageCeiling.NoCeiling, roleMenus: roleMenus);

        Assert.Contains(diff.DeleteRowIds, id => id == dupId);
        Assert.DoesNotContain(diff.DeleteRowIds, id => id == keepId);
        // 保留行已满足业务键 ⇒ 不重复补入
        Assert.DoesNotContain(diff.Inserts, u => u.RoleId == AdminRoleId && u.MenuId == StableSeedId.Menu("route:page"));
    }

    [Fact]
    public void SnowflakeManualRowWithBusinessKey_PreservedNotReinserted()
    {
        // 手工雪花行（非稳定主键）承载同业务键 ⇒ 保留原行，不插入稳定主键新行
        var manualSnowflakeId = 7300000000001234567L;
        var roleMenus = new List<TenantMenuSyncEngine.RoleMenuRowSnapshot>
        {
            new(manualSnowflakeId, AdminRoleId, StableSeedId.Menu("route:page")),
        };

        var diff = Compute(TenantMenuSyncPackageCeiling.NoCeiling, roleMenus: roleMenus);

        Assert.DoesNotContain(diff.Inserts, u => u.RoleId == AdminRoleId && u.MenuId == StableSeedId.Menu("route:page"));
        Assert.DoesNotContain(diff.DeleteRowIds, id => id == manualSnowflakeId);
    }

    [Fact]
    public void DeadRows_CleanedForAllRoleKinds()
    {
        var templateDead = 5111L;
        var customDead = 5222L;
        var roleMenus = new List<TenantMenuSyncEngine.RoleMenuRowSnapshot>
        {
            new(templateDead, AdminRoleId, 8888888888888L),
            new(customDead, CustomRoleId, 7777777777777L),
        };

        var diff = Compute(TenantMenuSyncPackageCeiling.NoCeiling, roleMenus: roleMenus);

        Assert.Contains(diff.DeleteRowIds, id => id == templateDead);
        Assert.Contains(diff.DeleteRowIds, id => id == customDead);
    }

    [Fact]
    public void DeterministicOrdering_OrderByRoleIdThenMenuId()
    {
        var diff = Compute(TenantMenuSyncPackageCeiling.NoCeiling);

        var sorted = diff.Inserts.OrderBy(u => u.RoleId).ThenBy(u => u.MenuId).ToArray();
        Assert.Equal(sorted, diff.Inserts);
    }
}
