using GameFrameX.Core.Const;
using GameFrameX.Core.Const.MenuSeed;

namespace GameFrameX.Core.Service.TenantMenuSync;

/// <summary>
/// 租户菜单同步引擎（C353 T3）。纯函数 diff：不读不写 DB，全部输入由调用方在出队执行期现场采集
/// （对齐 <see cref="GameFrameX.Core.Authorization.Diagnostics.MenuEndpointBindingDiagnosticsEngine"/> C128 纯函数先例）。
/// <para>三层权责：模板角色（Code 精确命中模板集且该 Code 命中角色数 = 1）按 SyncRoles 基线收敛、再被套餐 ceiling 夹取；
/// 自定义角色只夹逼不重写（仅删除种子域内超出 ceiling 的授权，不补入）；死行（MenuId 不在活种子集）全角色清理。</para>
/// <para>幂等与保留语义：期望行主键 = <see cref="StableSeedId.RoleMenu(roleCode, menuKey)"/> 确定性派生；
/// 已存在同 (RoleId, MenuId) 业务键的行（含雪花 Id 手工行）保留不重插，仅删除跨业务键重复 surplus 行。</para>
/// </summary>
public static class TenantMenuSyncEngine
{
    /// <summary>
    /// 租户角色快照（租户库 tenant_role_entity 行）
    /// </summary>
    public sealed record RoleSnapshot(long Id, string? Code);

    /// <summary>
    /// 租户角色菜单授权行快照（租户库 tenant_role_menu_entity 行）
    /// </summary>
    public sealed record RoleMenuRowSnapshot(long Id, long RoleId, long MenuId);

    /// <summary>
    /// 模板角色基线快照（来自 <see cref="TenantDefaultDataTemplates.RoleTemplates"/> 的稳定 Code 与 SyncRole）
    /// </summary>
    public sealed record RoleTemplateSnapshot(string Code, TenantMenuSyncRole SyncRole);

    /// <summary>
    /// 引擎输入（全部为出队执行期现场快照）
    /// </summary>
    public sealed record Input(
        long TenantId,
        TenantMenuSyncPackageCeiling Ceiling,
        IReadOnlyList<MenuSeedDefinition> Definitions,
        IReadOnlyList<RoleTemplateSnapshot> TemplateRoles,
        IReadOnlyList<RoleSnapshot> Roles,
        IReadOnlyList<RoleMenuRowSnapshot> RoleMenus);

    /// <summary>
    /// 待补入的授权行（Id 为确定性派生主键）
    /// </summary>
    public sealed record InsertRow(long Id, long RoleId, long MenuId);

    /// <summary>
    /// 引擎告警（不阻断同步，供状态行 / 日志观测）
    /// </summary>
    public sealed record Warning(string Code, string Message);

    /// <summary>
    /// 模板角色指派（角色快照 + 其命中的模板 SyncRole 基线）
    /// </summary>
    public sealed record TemplateAssignment(RoleSnapshot Role, TenantMenuSyncRole SyncRole);

    /// <summary>
    /// 引擎 diff 输出。确定性排序：Inserts 按 (RoleId, MenuId)，DeleteRowIds 升序。
    /// </summary>
    public sealed record Diff(
        IReadOnlyList<InsertRow> Inserts,
        IReadOnlyList<long> DeleteRowIds,
        IReadOnlyList<Warning> Warnings);

    /// <summary>
    /// 计算确定性 diff。
    /// </summary>
    public static Diff Compute(Input input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var definitions = input.Definitions;
        var seedDomainIds = definitions.Select(u => StableSeedId.Menu(u.Key)).ToHashSet();
        var keyByMenuId = new Dictionary<long, string>(definitions.Count);
        foreach (var definition in definitions)
        {
            keyByMenuId[StableSeedId.Menu(definition.Key)] = definition.Key;
        }

        // SAO 防御性剔除在闭包计算器层完成（lesson 2026-07-11 D1016：不信任套餐编辑入口）；
        // 引擎期望集保持纯 C189 语义（null 套餐 ⇒ 授权集不变，含 tenant-readable 辅助按钮）。

        // 模板角色判定：同租户内 Code 精确命中模板 Code 集合且命中数 = 1；
        // 命中数 != 1（租户自建同 Code 角色）⇒ 该 Code 组全部按自定义角色处理并告警（防冒名托管与稳定主键跨角色撞车）
        var templateByCode = input.TemplateRoles.ToDictionary(u => u.Code, StringComparer.Ordinal);
        var rolesByCode = input.Roles
                               .Where(u => u.Code != null && templateByCode.ContainsKey(u.Code))
                               .GroupBy(u => u.Code!, StringComparer.Ordinal)
                               .ToDictionary(u => u.Key, u => u.ToList(), StringComparer.Ordinal);

        var templateRoleIds = new HashSet<long>();
        var warnings = new List<Warning>();
        var templateAssignments = new List<TemplateAssignment>();
        foreach (var pair in rolesByCode)
        {
            if (pair.Value.Count == 1)
            {
                templateRoleIds.Add(pair.Value[0].Id);
                templateAssignments.Add(new TemplateAssignment(pair.Value[0], templateByCode[pair.Key].SyncRole));
            }
            else
            {
                warnings.Add(new Warning("duplicate-template-code",
                    $"角色 Code '{pair.Key}' 在租户 {input.TenantId} 命中 {pair.Value.Count} 个角色，整组按自定义角色处理（只夹逼不补入）。"));
            }
        }

        var customRoles = input.Roles.Where(u => !templateRoleIds.Contains(u.Id)).ToList();

        // 业务键去重：同 (RoleId, MenuId) 多行保留最小 Id 行，其余删除
        var deleteRowIds = new HashSet<long>();
        foreach (var duplicateGroup in input.RoleMenus.GroupBy(u => (u.RoleId, u.MenuId)).Where(u => u.Count() > 1))
        {
            foreach (var row in duplicateGroup.OrderBy(u => u.Id).Skip(1))
            {
                deleteRowIds.Add(row.Id);
            }
        }

        // 死行清理（全角色）：MenuId 不在当前活种子菜单集（C331 软删菜单残留）
        var rowsByRole = input.RoleMenus.ToLookup(u => u.RoleId);
        foreach (var row in input.RoleMenus)
        {
            if (!seedDomainIds.Contains(row.MenuId))
            {
                deleteRowIds.Add(row.Id);
            }
        }

        // 模板角色：期望集 = SelectSyncedDefinitions(role) ∩ ceiling（无 ceiling 即纯 C189 收敛）
        var inserts = new List<InsertRow>();
        foreach (var assignment in templateAssignments)
        {
            var role = assignment.Role;
            var syncRole = assignment.SyncRole;

            if (syncRole == TenantMenuSyncRole.None)
            {
                // C189 None 安全语义：零删除候选防误删（不补入、不删 surplus，仅全局死行清理覆盖）
                continue;
            }

            var expectedMenuIds = MenuSeedCatalog.GetSyncedMenuIds(definitions, syncRole)
                                                 .ToHashSet();
            if (input.Ceiling.Kind == TenantMenuSyncCeilingKind.Bounded)
            {
                expectedMenuIds.IntersectWith(input.Ceiling.MenuIds);
            }

            var existingMenuIds = new HashSet<long>();
            foreach (var row in rowsByRole[role.Id])
            {
                existingMenuIds.Add(row.MenuId);
                if (seedDomainIds.Contains(row.MenuId) && !expectedMenuIds.Contains(row.MenuId))
                {
                    deleteRowIds.Add(row.Id);
                }
            }

            foreach (var menuId in expectedMenuIds.Where(u => !existingMenuIds.Contains(u)))
            {
                inserts.Add(new InsertRow(StableSeedId.RoleMenu(role.Code!, keyByMenuId[menuId]), role.Id, menuId));
            }
        }

        // 自定义角色：只夹逼不重写（仅 Bounded；无 ceiling 不夹逼）
        if (input.Ceiling.Kind == TenantMenuSyncCeilingKind.Bounded)
        {
            foreach (var role in customRoles)
            {
                foreach (var row in rowsByRole[role.Id])
                {
                    if (seedDomainIds.Contains(row.MenuId) && !input.Ceiling.MenuIds.Contains(row.MenuId))
                    {
                        deleteRowIds.Add(row.Id);
                    }
                }
            }
        }

        return new Diff(
            inserts.OrderBy(u => u.RoleId).ThenBy(u => u.MenuId).ToArray(),
            deleteRowIds.OrderBy(u => u).ToArray(),
            warnings.OrderBy(u => u.Code).ThenBy(u => u.Message).ToArray());
    }
}
