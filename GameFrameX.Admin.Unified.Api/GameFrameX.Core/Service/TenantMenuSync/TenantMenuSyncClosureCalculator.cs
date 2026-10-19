using GameFrameX.Core.Const.MenuSeed;

namespace GameFrameX.Core.Service.TenantMenuSync;

/// <summary>
/// 套餐菜单闭包计算器（C353 T2）。纯函数：不读不写 DB，输入由调用方在出队执行期现场采集。
/// <para>闭包 = global_package_menu 行集 ∪ 祖先 ∪ 后代（按钮随父页面：勾选页面连带其下按钮，勾选按钮经祖先闭包带回其父页面）
/// ∩ 种子菜单域 − SuperAdmin-only 子树（防御性剔除，不信任套餐编辑入口，lesson 2026-07-11 D1016）。</para>
/// <para>特殊输入：PackageId == null ⇒ 无 ceiling（模板角色纯 C189 收敛、自定义角色不夹逼）；
/// 套餐悬空（不存在 / 软删 / 禁用）⇒ Dangling（跳过同步并置 failed，不删不补）；
/// 套餐存在而清单为空 ⇒ 空 Bounded（全部种子菜单授权收敛删除，有意为之的空套餐语义）。</para>
/// </summary>
public static class TenantMenuSyncClosureCalculator
{
    /// <summary>
    /// 计算套餐闭包。
    /// </summary>
    /// <param name="packageId">租户当前套餐 Id（null = 未分配套餐）</param>
    /// <param name="packageUsable">套餐行存在且未软删且启用（false 即悬空）</param>
    /// <param name="packageMenuIds">global_package_menu 行集的 MenuId 原始清单</param>
    /// <param name="definitions">当前活种子菜单定义（MenuSeedCatalog.AllDefinitions）</param>
    public static TenantMenuSyncPackageCeiling Compute(
        long? packageId,
        bool packageUsable,
        IReadOnlyList<long> packageMenuIds,
        IReadOnlyList<MenuSeedDefinition> definitions)
    {
        if (packageId == null)
        {
            return TenantMenuSyncPackageCeiling.NoCeiling;
        }

        if (!packageUsable)
        {
            return TenantMenuSyncPackageCeiling.Dangling;
        }

        var definitionById = new Dictionary<long, MenuSeedDefinition>(definitions.Count);
        var parentByKey = new Dictionary<string, string?>(definitions.Count, StringComparer.Ordinal);
        foreach (var definition in definitions)
        {
            definitionById[StableSeedId.Menu(definition.Key)] = definition;
            parentByKey[definition.Key] = definition.ParentKey;
        }

        var childrenByParent = definitions
                               .Where(u => u.ParentKey != null)
                               .GroupBy(u => u.ParentKey!, StringComparer.Ordinal)
                               .ToDictionary(u => u.Key, u => u.ToList(), StringComparer.Ordinal);

        // 祖先闭包：从每个清单 Key 向上走到根
        var closureKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var menuId in packageMenuIds)
        {
            if (!definitionById.TryGetValue(menuId, out var definition))
            {
                continue;
            }

            var current = definition.Key;
            while (current != null && closureKeys.Add(current))
            {
                current = parentByKey.TryGetValue(current, out var parent) ? parent : null;
            }
        }

        // 后代闭包：按钮随父页面（页面入集则其下按钮一并入集）
        var queue = new Queue<string>(closureKeys);
        while (queue.Count > 0)
        {
            var key = queue.Dequeue();
            if (!childrenByParent.TryGetValue(key, out var children))
            {
                continue;
            }

            foreach (var child in children)
            {
                if (closureKeys.Add(child.Key))
                {
                    queue.Enqueue(child.Key);
                }
            }
        }

        var superAdminOnlyIds = MenuSeedCatalog.GetSuperAdminOnlySubtreeMenuIds(definitions);
        var closureMenuIds = closureKeys
                             .Select(StableSeedId.Menu)
                             .Where(u => !superAdminOnlyIds.Contains(u))
                             .ToHashSet();

        return TenantMenuSyncPackageCeiling.Bounded(closureMenuIds);
    }
}
