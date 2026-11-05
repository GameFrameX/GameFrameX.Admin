using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Core.Enum.Errors;
using GameFrameX.Core.Exceptions;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Tests.ServerCenter.GameAreaMerge;

public sealed class GameAreaMergeServiceTests
{
    [Fact]
    public async Task Workflow_PersistsEveryPhase_AndIgnoresDisconnectedRequest()
    {
        using (var context = new GameAreaMergeTestContext())
        using (var disconnected = new CancellationTokenSource())
        {
            var plan = await context.CreateAsync();
            Assert.Equal(GameAreaMergePlanStatus.Draft, plan.Status);
            plan = await context.Service.DryRunAsync(new() { Id = plan.Id });
            Assert.Equal(GameAreaMergePlanStatus.DryRunCompleted, plan.Status);
            Assert.NotNull(plan.DryRunReportSnapshot);
            context.Login(12);
            plan = await context.Service.ApproveAsync(new() { Id = plan.Id, Reason = "复核", ReviewerOperatorId = 12 });
            Assert.Equal(GameAreaMergePlanStatus.Approved, plan.Status);
            context.Accessor.HttpContext!.RequestAborted = disconnected.Token;
            context.Handler.OnExecute = () =>
            {
                disconnected.Cancel();
                Assert.Equal(GameAreaMergePlanStatus.Executing, context.Database.Queryable<TenantGameAreaMergePlanEntity>().InSingle(plan.Id).Status);
            };
            plan = await context.Service.ExecuteAsync(new() { Id = plan.Id, BackupConfirmed = true, BackupIdentifier = "backup-001" });
            Assert.Equal(GameAreaMergePlanStatus.Completed, plan.Status);
            Assert.NotNull(plan.CompletedTime);
            Assert.NotNull(plan.ExecutionStartedTime);
            Assert.Equal("backup-001", plan.BackupIdentifier);
            Assert.Equal(11, plan.ApplicantOperatorId);
            Assert.Equal(12, plan.ReviewerOperatorId);
            Assert.Equal(new[] { "plan_server_merge", "dry_run_server_merge", "execute_server_merge" }, context.Handler.Actions);
        }
    }

    [Theory]
    [InlineData(11, 11)]
    [InlineData(12, 11)]
    [InlineData(12, 0)]
    [InlineData(12, 13)]
    public async Task Approve_RejectsSelfMissingOrForgedReviewer(long currentOperator, long reviewer)
    {
        using (var context = new GameAreaMergeTestContext())
        {
            var plan = await context.CreateAsync();
            await context.Service.DryRunAsync(new() { Id = plan.Id });
            context.Login(currentOperator);
            var exception = await Assert.ThrowsAsync<BusinessException>(() => context.Service.ApproveAsync(new() { Id = plan.Id, ReviewerOperatorId = reviewer, Reason = "复核" }));
            Assert.Equal((int)AreaMergeErrorCode.ReviewRequired, exception.Code);
        }
    }

    [Fact]
    public async Task Approve_RejectsGlobalDomainWrites()
    {
        using (var context = new GameAreaMergeTestContext())
        {
            var plan = await context.CreateAsync();
            context.Handler.GlobalRowsTouched = 1;
            await context.Service.DryRunAsync(new() { Id = plan.Id });
            context.Login(12);
            var exception = await Assert.ThrowsAsync<BusinessException>(() => context.Service.ApproveAsync(new() { Id = plan.Id, ReviewerOperatorId = 12, Reason = "复核" }));
            Assert.Equal((int)AreaMergeErrorCode.GlobalDomainTouched, exception.Code);
        }
    }

    [Theory]
    [InlineData(false, "backup")]
    [InlineData(true, " ")]
    public async Task Execute_RequiresConfirmedBackup(bool confirmed, string identifier)
    {
        using (var context = new GameAreaMergeTestContext())
        {
            var plan = await context.ApproveAsync();
            var exception = await Assert.ThrowsAsync<BusinessException>(() => context.Service.ExecuteAsync(new() { Id = plan.Id, BackupConfirmed = confirmed, BackupIdentifier = identifier }));
            Assert.Equal((int)AreaMergeErrorCode.BackupRequired, exception.Code);
            Assert.DoesNotContain("execute_server_merge", context.Handler.Actions);
        }
    }

    [Fact]
    public async Task Execute_AlreadyExecutedReplay_CompletesAndDoesNotDispatchAgain()
    {
        using (var context = new GameAreaMergeTestContext())
        {
            var plan = await context.ApproveAsync();
            context.Handler.AlreadyExecuted = true;
            var completed = await context.Service.ExecuteAsync(new() { Id = plan.Id, BackupConfirmed = true, BackupIdentifier = "backup" });
            Assert.Equal(GameAreaMergePlanStatus.Completed, completed.Status);
            Assert.Contains("\"AlreadyExecuted\":true", completed.ExecutionReportSnapshot);
            await context.Service.ExecuteAsync(new() { Id = plan.Id });
            Assert.Single(context.Handler.Actions, action => action == "execute_server_merge");
        }
    }

    [Theory]
    [InlineData("dry_run_server_merge")]
    [InlineData("execute_server_merge")]
    public async Task FailedDispatch_IsPersisted_AndRequiresFreshRehearsalAndApproval(string action)
    {
        using (var context = new GameAreaMergeTestContext())
        {
            var plan = action == "dry_run_server_merge" ? await context.CreateAsync() : await context.ApproveAsync();
            context.Handler.FailAction = action;
            if (action == "dry_run_server_merge")
            {
                await Assert.ThrowsAsync<HttpRequestException>(() => context.Service.DryRunAsync(new() { Id = plan.Id }));
            }
            else
            {
                await Assert.ThrowsAsync<HttpRequestException>(() => context.Service.ExecuteAsync(new() { Id = plan.Id, BackupConfirmed = true, BackupIdentifier = "backup" }));
            }
            var failed = await context.Service.DetailAsync(new() { Id = plan.Id });
            Assert.Equal(GameAreaMergePlanStatus.Failed, failed.Status);
            Assert.NotNull(failed.FailedTime);
            await Assert.ThrowsAsync<BusinessException>(() => context.Service.ExecuteAsync(new() { Id = plan.Id, BackupConfirmed = true, BackupIdentifier = "backup" }));
            context.Handler.FailAction = null;
            var rehearsed = await context.Service.DryRunAsync(new() { Id = plan.Id });
            Assert.Equal(GameAreaMergePlanStatus.DryRunCompleted, rehearsed.Status);
            Assert.Null(rehearsed.ReviewerOperatorId);
            Assert.False(rehearsed.BackupConfirmed);
        }
    }

    [Theory]
    [InlineData(GameAreaMergePlanStatus.Draft)]
    [InlineData(GameAreaMergePlanStatus.DryRunCompleted)]
    [InlineData(GameAreaMergePlanStatus.Approved)]
    [InlineData(GameAreaMergePlanStatus.Executing)]
    [InlineData(GameAreaMergePlanStatus.Completed)]
    [InlineData(GameAreaMergePlanStatus.Failed)]
    public async Task StateMachine_RejectsDisallowedTransitions(GameAreaMergePlanStatus state)
    {
        using (var context = new GameAreaMergeTestContext())
        {
            var plan = await context.CreateAsync();
            await context.Database.Updateable<TenantGameAreaMergePlanEntity>().SetColumns(item => item.Status == state).Where(item => item.Id == plan.Id).ExecuteCommandAsync();
            if (state != GameAreaMergePlanStatus.Draft && state != GameAreaMergePlanStatus.Failed)
            {
                await Assert.ThrowsAsync<BusinessException>(() => context.Service.DryRunAsync(new() { Id = plan.Id }));
            }
            if (state != GameAreaMergePlanStatus.DryRunCompleted)
            {
                context.Login(12);
                await Assert.ThrowsAsync<BusinessException>(() => context.Service.ApproveAsync(new() { Id = plan.Id, ReviewerOperatorId = 12, Reason = "复核" }));
            }
            if (state != GameAreaMergePlanStatus.Approved && state != GameAreaMergePlanStatus.Completed)
            {
                await Assert.ThrowsAsync<BusinessException>(() => context.Service.ExecuteAsync(new() { Id = plan.Id, BackupConfirmed = true, BackupIdentifier = "backup" }));
            }
        }
    }

    [Theory]
    [InlineData(101, 101)]
    [InlineData(101, 999)]
    [InlineData(999, 102)]
    [InlineData(0, 102)]
    public async Task Create_RejectsInvalidServers(long target, long source)
    {
        using (var context = new GameAreaMergeTestContext())
        {
            var exception = await Assert.ThrowsAsync<BusinessException>(() => context.Service.AddAsync(new() { TargetServerId = target, SourceServerIds = new() { source }, Reason = "合服" }));
            Assert.Equal((int)AreaMergeErrorCode.InvalidServers, exception.Code);
        }
    }

    [Fact]
    public void PlanEntity_UsesMainDatabaseAndExplicitTenantPhysicalOwnership()
    {
        var entityType = typeof(TenantGameAreaMergePlanEntity);
        Assert.True(GameFrameX.Core.SqlSugar.SqlSugarEntityBoundary.IsMainDatabaseTable(entityType));
        Assert.Equal(GameFrameX.Core.SqlSugar.DatabaseOwnership.Tenant, GameFrameX.Core.SqlSugar.SqlSugarEntityBoundary.GetOwnership(entityType));
        Assert.Equal("tenant_game_area_merge_plan_entity", GameFrameX.Core.SqlSugar.SqlSugarEntityBoundary.GetPhysicalTableName(entityType, entityType.Name));
    }

    [Fact]
    public async Task Create_RejectsReservedSource()
    {
        using (var context = new GameAreaMergeTestContext())
        {
            await context.CreateAsync();
            var exception = await Assert.ThrowsAsync<BusinessException>(() => context.CreateAsync());
            Assert.Equal((int)AreaMergeErrorCode.SourceReserved, exception.Code);
        }
    }
}
