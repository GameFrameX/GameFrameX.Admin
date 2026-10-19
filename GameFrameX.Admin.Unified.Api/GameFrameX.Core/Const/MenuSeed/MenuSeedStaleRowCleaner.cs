using GameFrameX.Entity.Global;
using SqlSugar;

namespace GameFrameX.Core.Const.MenuSeed;

/// <summary>
/// global_menu 种子演化残留行启动自愈清理（C331 / GFX-741）。
/// <para>global_menu 的写入方只有两处：GlobalMenuSeedData 种子 upsert 与 ApiEndpointCatalogSync 的
/// permission:auto:* 运行期行。因此「有效行中 Key 既不在当前种子集、又非 permission:auto: 前缀」即为
/// 种子演化残留（典型来源：D1 禁简写改名后旧 Key 行不再被按主键 upsert 命中而留存）；
/// 「Key 命中种子但 Id ≠ 种子权威 Id」是同因的另一形态：旧布局 Id 行不被按主键 upsert 命中，
/// 权威 Id 行插入后同 Key 双行并存（典型来源：StableSeedId 紧凑布局切换前的旧雪花 Id 行残留）。</para>
/// </summary>
public static class MenuSeedStaleRowCleaner
{
    /// <summary>
    /// ApiEndpointCatalogSync 运行期管理的自动权限行 Key 前缀。这些行随活端点增减自管生命周期
    /// （含软删后复活），不属于种子残留，清理必须豁免。
    /// </summary>
    private const string AutoManagedKeyPrefix = "permission:auto:";

    /// <summary>
    /// 软删 global_menu 有效行中的种子演化残留行（旧 Key 行 + 同 Key 异 Id 重复行），
    /// 并把因此父级悬空的自动权限行 Pid 归 0。
    /// <para>调用时机：主库种子 upsert 完成后、<see cref="MenuSeedCatalog.LoadFromDb"/> 之前——
    /// 残留行与新 Key 行同 Permission / 同 Key 并存会让 ValidateDbMenus 启动崩溃（GFX-741），必须先清理后校验。</para>
    /// <para>同 Key 异 Id 重复行仅在种子权威 Id 行在场时判定为残留：权威行缺席（如本次启动未执行 upsert）
    /// 时保留该行，避免误删该 Key 唯一的有效菜单定义。</para>
    /// <para>Pid 归 0 依据：ApiEndpointCatalogSync 每次启动会按 controllerPidMap 把 pid=0 的自动按钮
    /// 重新挂到所属页面菜单，挂根只是校验前的临时安全态。</para>
    /// <para>幂等：残留行软删后对后续查询不可见（IsDeleted 过滤），重复调用不再命中任何行。</para>
    /// </summary>
    /// <param name="db">主库连接。</param>
    /// <param name="seedIdsByKey">当前 GlobalMenuSeedData 种子的 Key → 权威 Id（StableSeedId.Menu）映射。</param>
    /// <returns>本次软删的残留行 Key 列表（已排序，供启动日志输出定位）。</returns>
    public static IReadOnlyList<string> RemoveStaleRows(ISqlSugarClient db, IReadOnlyDictionary<string, long> seedIdsByKey)
    {
        var validRows = db.Queryable<GlobalMenuEntity>()
                          .Where(u => u.IsDeleted == false)
                          .ToList();

        var activeIds = validRows.Select(u => u.Id).ToHashSet();
        var staleRows = validRows
                        .Where(u => !(u.Key ?? string.Empty).StartsWith(AutoManagedKeyPrefix, StringComparison.Ordinal))
                        .Where(u => IsEvolutionResidue(u, seedIdsByKey, activeIds))
                        .ToList();
        if (staleRows.Count == 0)
        {
            return Array.Empty<string>();
        }

        var staleIds = staleRows.Select(u => u.Id).ToList();
        db.Updateable<GlobalMenuEntity>()
          .SetColumns(u => new GlobalMenuEntity { IsDeleted = true, })
          .Where(u => staleIds.Contains(u.Id))
          .ExecuteCommand();

        // 残留父级被清理后悬空的自动权限行（含清理前即已悬空的）归 0 挂根，
        // 否则 ValidateDbMenus 的「父级不存在」校验会拦下启动。
        var validIdSet = validRows.Select(u => u.Id).ToHashSet();
        foreach (var staleId in staleIds)
        {
            validIdSet.Remove(staleId);
        }

        var danglingAutoIds = validRows
                              .Where(u => (u.Key ?? string.Empty).StartsWith(AutoManagedKeyPrefix, StringComparison.Ordinal))
                              .Where(u => u.Pid != 0 && !validIdSet.Contains(u.Pid))
                              .Select(u => u.Id)
                              .ToList();
        if (danglingAutoIds.Count > 0)
        {
            db.Updateable<GlobalMenuEntity>()
              .SetColumns(u => new GlobalMenuEntity { Pid = 0, })
              .Where(u => danglingAutoIds.Contains(u.Id))
              .ExecuteCommand();
        }

        return staleRows.Select(u => u.Key ?? string.Empty)
                        .OrderBy(u => u, StringComparer.Ordinal)
                        .ToArray();
    }

    /// <summary>
    /// 判定有效行是否为种子演化残留：Key 不在当前种子（旧 Key 残留），或 Key 命中种子但 Id 漂移
    /// 且种子权威 Id 行在场（同 Key 异 Id 重复行，权威行在场才清理，防误删唯一有效定义）。
    /// </summary>
    private static bool IsEvolutionResidue(GlobalMenuEntity row, IReadOnlyDictionary<string, long> seedIdsByKey, HashSet<long> activeIds)
    {
        var key = row.Key ?? string.Empty;
        if (!seedIdsByKey.TryGetValue(key, out var seedId))
        {
            return true;
        }

        return row.Id != seedId && activeIds.Contains(seedId);
    }
}
