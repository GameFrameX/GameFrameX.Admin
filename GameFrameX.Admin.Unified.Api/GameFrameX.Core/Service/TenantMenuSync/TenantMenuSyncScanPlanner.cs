using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Service.TenantMenuSync;

/// <summary>
/// 租户菜单同步扫描计划器（C353 T6）。纯函数：给定「存活启用 Db 租户（默认租户已在上游枚举排除）+ 状态行 + 当前目录版本 + 当前时间」，
/// 输出待入队租户与原因。启动比对与周期兜底扫描共用同一口径。
/// <para>规则：状态行缺失 ⇒ 待同步（首启全量 / 新建租户一个扫描周期内同步由此成立）；
/// LastCatalogVersion ≠ 当前版本 ⇒ 待同步；Status == Failed 且 NextRetryTime 已到（或为空）⇒ 待重试（失败自愈闭环）。</para>
/// </summary>
public static class TenantMenuSyncScanPlanner
{
    /// <summary>
    /// 状态行投影（主库 system_tenant_menu_sync_state；行缺失即无对应元素）
    /// </summary>
    public sealed record StateRow(long TenantId, string LastCatalogVersion, TenantMenuSyncStatusEnum Status, long? NextRetryTime);

    /// <summary>
    /// 入队计划项
    /// </summary>
    public sealed record PlanItem(long TenantId, string Reason);

    /// <summary>
    /// 计算待入队集合。
    /// </summary>
    /// <param name="eligibleTenantIds">存活启用 Db 租户 Id（GetTenantDbListAsync 同口径；C354 起默认租户纳入）</param>
    /// <param name="stateRows">状态行集合</param>
    /// <param name="currentCatalogVersion">当前 MenuSeedCatalog.CatalogVersion</param>
    /// <param name="nowUnixSeconds">当前秒级 Unix 时间戳</param>
    public static IReadOnlyList<PlanItem> Plan(
        IReadOnlyList<long> eligibleTenantIds,
        IReadOnlyList<StateRow> stateRows,
        string currentCatalogVersion,
        long nowUnixSeconds)
    {
        var stateByTenant = stateRows.ToDictionary(u => u.TenantId);

        var plan = new List<PlanItem>();
        foreach (var tenantId in eligibleTenantIds)
        {
            if (!stateByTenant.TryGetValue(tenantId, out var state))
            {
                plan.Add(new PlanItem(tenantId, "state-missing"));
                continue;
            }

            if (!string.Equals(state.LastCatalogVersion, currentCatalogVersion, StringComparison.Ordinal))
            {
                plan.Add(new PlanItem(tenantId, "catalog-lagged"));
                continue;
            }

            if (state.Status == TenantMenuSyncStatusEnum.Failed
                && (state.NextRetryTime == null || state.NextRetryTime <= nowUnixSeconds))
            {
                plan.Add(new PlanItem(tenantId, "retry-due"));
            }
        }

        return plan;
    }
}
