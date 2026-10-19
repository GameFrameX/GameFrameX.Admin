using GameFrameX.Core.Const.MenuSeed;

namespace GameFrameX.Core.Service.TenantMenuSync;

/// <summary>
/// 套餐菜单清单闭包补全（C354）。纯函数：不读不写 DB，输入由调用方现场采集。
/// <para>补祖先目录：勾选子页面自动带祖先目录后落库（编辑期所见即所得，幂等——已含祖先的输入无变化）。
/// 祖先回溯不越过 SuperAdmin-only 子树边界：tenant-readable 辅助按钮的父链是超管目录，补祖先会把
/// 超管菜单带回清单，故遇 SAO 祖先即停（与闭包计算器的 SAO 防御性剔除一致，lesson 2026-07-11 D1016）。</para>
/// <para>Enterprise 全集：全部非 SuperAdmin-only 子树种子菜单（含按钮）∪ tenant-readable 辅助按钮，
/// 即「种子域 −（SAO 子树 − tenant-readable）」；作为默认租户保留套餐的启动幂等补全目标
/// （种子演化跟随，SuperAdmin 不可手工收窄默认租户天花板）。</para>
/// </summary>
public static class TenantPackageMenuClosure
{
    /// <summary>
    /// 补全祖先目录闭包（不越过 SAO 子树边界；清单外的非种子 Id 原样保留）。确定性排序输出。
    /// </summary>
    /// <param name="menuIds">套餐菜单 Id 原始清单</param>
    /// <param name="definitions">当前活种子菜单定义（MenuSeedCatalog.AllDefinitions）</param>
    public static IReadOnlyList<long> CompleteAncestors(IReadOnlyList<long> menuIds, IReadOnlyList<MenuSeedDefinition> definitions)
    {
        var definitionById = new Dictionary<long, MenuSeedDefinition>(definitions.Count);
        var parentByKey = new Dictionary<string, string?>(definitions.Count, StringComparer.Ordinal);
        foreach (var definition in definitions)
        {
            definitionById[StableSeedId.Menu(definition.Key)] = definition;
            parentByKey[definition.Key] = definition.ParentKey;
        }

        var superAdminOnlyIds = MenuSeedCatalog.GetSuperAdminOnlySubtreeMenuIds(definitions);
        var closure = new HashSet<long>();
        foreach (var menuId in menuIds)
        {
            closure.Add(menuId);
            if (superAdminOnlyIds.Contains(menuId))
            {
                // SAO 子树成员（tenant-readable 按钮）：父链是超管目录，不回溯祖先
                continue;
            }

            var current = definitionById.TryGetValue(menuId, out var definition) ? definition.Key : null;
            while (current != null)
            {
                if (!parentByKey.TryGetValue(current, out var parentKey) || parentKey == null)
                {
                    break;
                }

                var parentId = StableSeedId.Menu(parentKey);
                if (superAdminOnlyIds.Contains(parentId))
                {
                    // 祖先进入 SAO 子树即停，不把超管目录带回清单
                    break;
                }

                closure.Add(parentId);
                current = parentKey;
            }
        }

        return closure.OrderBy(u => u, Comparer<long>.Default).ToArray();
    }

    /// <summary>
    /// Enterprise 套餐菜单全集（启动幂等补全目标）：全部非 SAO 子树种子定义 ∪ tenant-readable 辅助按钮。
    /// </summary>
    /// <param name="definitions">当前活种子菜单定义（MenuSeedCatalog.AllDefinitions）</param>
    public static IReadOnlyList<long> EnterprisePackageMenuIds(IReadOnlyList<MenuSeedDefinition> definitions)
    {
        var superAdminOnlyIds = MenuSeedCatalog.GetSuperAdminOnlySubtreeMenuIds(definitions);
        var tenantReadableIds = definitions
                                .Where(u => u.SuperAdminOnly && u.AllowTenantWhenSuperAdminOnly)
                                .Select(u => StableSeedId.Menu(u.Key))
                                .ToHashSet();

        return definitions
               .Select(u => StableSeedId.Menu(u.Key))
               .Where(u => !superAdminOnlyIds.Contains(u) || tenantReadableIds.Contains(u))
               .OrderBy(u => u, Comparer<long>.Default)
               .ToArray();
    }
}
