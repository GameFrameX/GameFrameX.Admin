using GameFrameX.Core.Service.TenantMenuSync;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Tests.TenantMenuSync;

/// <summary>
/// C353 T6：扫描计划器纯函数矩阵（state-missing / catalog-lagged / retry-due / up-to-date）。
/// </summary>
public class TenantMenuSyncScanPlannerTests
{
    private const string CurrentVersion = "v-current";
    private const long Now = 1_800_000_000;

    private static TenantMenuSyncScanPlanner.StateRow State(long tenantId, string version, TenantMenuSyncStatusEnum status, long? nextRetry)
    {
        return new TenantMenuSyncScanPlanner.StateRow(tenantId, version, status, nextRetry);
    }

    [Fact]
    public void MissingStateRow_PlannedAsStateMissing()
    {
        var plan = TenantMenuSyncScanPlanner.Plan(
            new[] { 1001L, 1002L },
            new[] { State(1002L, CurrentVersion, TenantMenuSyncStatusEnum.Succeeded, null) },
            CurrentVersion,
            Now);

        var item = Assert.Single(plan);
        Assert.Equal(1001L, item.TenantId);
        Assert.Equal("state-missing", item.Reason);
    }

    [Fact]
    public void LaggedCatalogVersion_PlannedAsCatalogLagged()
    {
        var plan = TenantMenuSyncScanPlanner.Plan(
            new[] { 1001L },
            new[] { State(1001L, "v-old", TenantMenuSyncStatusEnum.Succeeded, null) },
            CurrentVersion,
            Now);

        var item = Assert.Single(plan);
        Assert.Equal("catalog-lagged", item.Reason);
    }

    [Fact]
    public void FailedWithDueOrEmptyRetryTime_PlannedAsRetryDue()
    {
        var plan = TenantMenuSyncScanPlanner.Plan(
            new[] { 1001L, 1002L },
            new[]
            {
                State(1001L, CurrentVersion, TenantMenuSyncStatusEnum.Failed, Now - 1),
                State(1002L, CurrentVersion, TenantMenuSyncStatusEnum.Failed, null),
            },
            CurrentVersion,
            Now);

        Assert.Equal(2, plan.Count);
        Assert.All(plan, item => Assert.Equal("retry-due", item.Reason));
    }

    [Fact]
    public void FailedWithFutureRetryTime_Skipped()
    {
        var plan = TenantMenuSyncScanPlanner.Plan(
            new[] { 1001L },
            new[] { State(1001L, CurrentVersion, TenantMenuSyncStatusEnum.Failed, Now + 600) },
            CurrentVersion,
            Now);

        Assert.Empty(plan);
    }

    [Fact]
    public void UpToDateSucceeded_Skipped()
    {
        var plan = TenantMenuSyncScanPlanner.Plan(
            new[] { 1001L },
            new[] { State(1001L, CurrentVersion, TenantMenuSyncStatusEnum.Succeeded, null) },
            CurrentVersion,
            Now);

        Assert.Empty(plan);
    }

    [Fact]
    public void StateRowForVanishedTenant_Ignored()
    {
        // 状态行残留而租户已删（清理钩子漏网）：枚举口径不含 ⇒ 不入队
        var plan = TenantMenuSyncScanPlanner.Plan(
            Array.Empty<long>(),
            new[] { State(9999L, "v-old", TenantMenuSyncStatusEnum.Failed, null) },
            CurrentVersion,
            Now);

        Assert.Empty(plan);
    }
}
