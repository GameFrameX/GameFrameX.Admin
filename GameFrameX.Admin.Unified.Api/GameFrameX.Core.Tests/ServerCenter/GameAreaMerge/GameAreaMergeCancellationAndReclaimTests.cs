using GameFrameX.Application.ServerCenter.GameAreaMerge.Dto;
using GameFrameX.Core.Enum.Errors;
using GameFrameX.Core.Exceptions;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Core.Tests.ServerCenter.GameAreaMerge;

public sealed class GameAreaMergeCancellationAndReclaimTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Cancel_RequiresReason(string reason)
    {
        using (var context = new GameAreaMergeTestContext())
        {
            var plan = await context.CreateAsync();

            var exception = await Assert.ThrowsAsync<BusinessException>(() => context.Service.CancelAsync(new() { Id = plan.Id, Reason = reason }));

            Assert.Equal((int)AreaMergeErrorCode.ReviewRequired, exception.Code);
        }
    }

    [Theory]
    [InlineData(GameAreaMergePlanStatus.Draft)]
    [InlineData(GameAreaMergePlanStatus.DryRunCompleted)]
    [InlineData(GameAreaMergePlanStatus.Approved)]
    [InlineData(GameAreaMergePlanStatus.Failed)]
    public async Task Cancel_AllNeverExecutedStates_Succeeds(GameAreaMergePlanStatus state)
    {
        using (var context = new GameAreaMergeTestContext())
        {
            var plan = await context.CreateAsync();
            if (state == GameAreaMergePlanStatus.DryRunCompleted)
            {
                plan = await context.Service.DryRunAsync(new() { Id = plan.Id });
            }
            else if (state == GameAreaMergePlanStatus.Approved)
            {
                await context.Service.DryRunAsync(new() { Id = plan.Id });
                context.Login(12);
                plan = await context.Service.ApproveAsync(new() { Id = plan.Id, ReviewerOperatorId = 12, Reason = "复核" });
            }
            else if (state == GameAreaMergePlanStatus.Failed)
            {
                await context.Database.Updateable<TenantGameAreaMergePlanEntity>()
                    .SetColumns(item => new TenantGameAreaMergePlanEntity
                    {
                        Status = GameAreaMergePlanStatus.Failed,
                        FailedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    })
                    .Where(item => item.Id == plan.Id)
                    .ExecuteCommandAsync();
            }

            var cancelled = await context.Service.CancelAsync(new() { Id = plan.Id, Reason = "取消错误的区服范围" });

            Assert.Equal(GameAreaMergePlanStatus.Cancelled, cancelled.Status);
            Assert.Equal(state == GameAreaMergePlanStatus.Approved ? 12 : 11, cancelled.CancelledByOperatorId);
            Assert.NotNull(cancelled.CancelledTime);
            Assert.Equal("取消错误的区服范围", cancelled.CancellationReason);
            Assert.False(cancelled.Cancellable);
        }
    }

    [Theory]
    [InlineData(GameAreaMergePlanStatus.Executing, true)]
    [InlineData(GameAreaMergePlanStatus.Completed, false)]
    [InlineData(GameAreaMergePlanStatus.Cancelled, false)]
    public async Task Cancel_ExecutedOrTerminalStates_IsRejected(GameAreaMergePlanStatus state, bool enteredExecution)
    {
        using (var context = new GameAreaMergeTestContext())
        {
            var plan = await context.CreateAsync();
            await context.Database.Updateable<TenantGameAreaMergePlanEntity>()
                .SetColumns(item => new TenantGameAreaMergePlanEntity
                {
                    Status = state,
                    ExecutionStartedTime = enteredExecution ? 1 : null,
                })
                .Where(item => item.Id == plan.Id)
                .ExecuteCommandAsync();

            var exception = await Assert.ThrowsAsync<BusinessException>(() => context.Service.CancelAsync(new() { Id = plan.Id, Reason = "取消" }));

            Assert.Equal((int)AreaMergeErrorCode.CancellationNotAllowed, exception.Code);
        }
    }

    [Fact]
    public async Task Cancel_ReleasesAllParticipatingServersForAnotherPlan()
    {
        using (var context = new GameAreaMergeTestContext())
        {
            var plan = await context.CreateAsync();
            await context.Service.CancelAsync(new() { Id = plan.Id, Reason = "重新选择区服" });
            // 测试进程的 SqlSugar fallback generator 在同一 SQLite 连接上会复用 ID；删除已核验的取消行只为释放该测试 ID。
            await context.Database.Deleteable<TenantGameAreaMergePlanEntity>()
                .Where(item => item.Id == plan.Id)
                .ExecuteCommandAsync();

            var replacement = await context.CreateAsync();

            Assert.Equal(GameAreaMergePlanStatus.Draft, replacement.Status);
            Assert.Equal(plan.TargetServerId, replacement.TargetServerId);
            Assert.Equal(plan.SourceServerIds, replacement.SourceServerIds);
        }
    }

    [Fact]
    public async Task CancelAndExecuteConcurrently_OnlyOneOperationClaimsPlan()
    {
        using (var context = new GameAreaMergeTestContext())
        {
            var plan = await context.ApproveAsync();
            var cancelTask = TryCancelAsync(context, plan.Id);
            var executeTask = TryExecuteAsync(context, plan.Id);
            var results = await Task.WhenAll(cancelTask, executeTask);

            Assert.Single(results, result => result);
            var stored = await context.Service.DetailAsync(new() { Id = plan.Id });
            Assert.Contains(stored.Status, new[] { GameAreaMergePlanStatus.Cancelled, GameAreaMergePlanStatus.Completed });
        }
    }

    [Fact]
    public async Task Execute_ExecutingBeforeTimeout_IsRejected()
    {
        using (var context = new GameAreaMergeTestContext())
        {
            var plan = await context.ApproveAsync();
            await SetExecutionStateAsync(context, plan.Id, DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            var exception = await Assert.ThrowsAsync<BusinessException>(() => context.Service.ExecuteAsync(new()
            {
                Id = plan.Id, BackupConfirmed = true, BackupIdentifier = "backup-current",
            }));

            Assert.Equal((int)AreaMergeErrorCode.InvalidState, exception.Code);
            Assert.DoesNotContain("execute_server_merge", context.Handler.Actions);
        }
    }

    [Fact]
    public async Task Execute_StaleExecuting_ReclaimsAndReplaysAlreadyExecuted()
    {
        using (var context = new GameAreaMergeTestContext())
        {
            var plan = await context.ApproveAsync();
            await SetExecutionStateAsync(context, plan.Id, 1);
            context.Handler.AlreadyExecuted = true;

            var completed = await context.Service.ExecuteAsync(new()
            {
                Id = plan.Id, BackupConfirmed = true, BackupIdentifier = "backup-replay",
            });

            Assert.Equal(GameAreaMergePlanStatus.Completed, completed.Status);
            Assert.True(completed.ExecutionReclaimable == false);
            Assert.Contains("\"AlreadyExecuted\":true", completed.ExecutionReportSnapshot);
        }
    }

    [Fact]
    public async Task Execute_TwoStaleReclaims_OnlyOneAtomicClaimSucceeds()
    {
        using (var context = new GameAreaMergeTestContext())
        {
            var plan = await context.ApproveAsync();
            await SetExecutionStateAsync(context, plan.Id, 1);
            using (var firstRequestEntered = new ManualResetEventSlim())
            using (var releaseFirstRequest = new ManualResetEventSlim())
            {
                context.Handler.OnExecute = () =>
                {
                    firstRequestEntered.Set();
                    releaseFirstRequest.Wait(TimeSpan.FromSeconds(5));
                };
                var first = TryExecuteAsync(context, plan.Id);
                Assert.True(firstRequestEntered.Wait(TimeSpan.FromSeconds(5)));
                var second = TryExecuteAsync(context, plan.Id);
                await Task.Delay(50);
                releaseFirstRequest.Set();
                var results = await Task.WhenAll(first, second);

                Assert.Contains(true, results);
                Assert.Single(context.Handler.Actions, action => action == "execute_server_merge");
                Assert.Equal(GameAreaMergePlanStatus.Completed, (await context.Service.DetailAsync(new() { Id = plan.Id })).Status);
            }
        }
    }

    [Theory]
    [InlineData(GameAreaMergePlanStatus.Draft, true, false)]
    [InlineData(GameAreaMergePlanStatus.DryRunCompleted, true, false)]
    [InlineData(GameAreaMergePlanStatus.Approved, true, false)]
    [InlineData(GameAreaMergePlanStatus.Executing, false, false)]
    [InlineData(GameAreaMergePlanStatus.Completed, false, false)]
    [InlineData(GameAreaMergePlanStatus.Failed, true, false)]
    [InlineData(GameAreaMergePlanStatus.Cancelled, false, false)]
    public async Task VisibilityFields_AreComputedByServer(GameAreaMergePlanStatus state, bool cancellable, bool executionReclaimable)
    {
        using (var context = new GameAreaMergeTestContext())
        {
            var plan = await context.CreateAsync();
            await context.Database.Updateable<TenantGameAreaMergePlanEntity>()
                .SetColumns(item => new TenantGameAreaMergePlanEntity
                {
                    Status = state,
                    ExecutionStartedTime = state == GameAreaMergePlanStatus.Executing ? DateTimeOffset.UtcNow.ToUnixTimeSeconds() : null,
                })
                .Where(item => item.Id == plan.Id)
                .ExecuteCommandAsync();

            var response = await context.Service.DetailAsync(new() { Id = plan.Id });

            Assert.Equal(cancellable, response.Cancellable);
            Assert.Equal(executionReclaimable, response.ExecutionReclaimable);
        }
    }

    [Fact]
    public async Task VisibilityFields_ExecutingAfterTimeout_IsReclaimable()
    {
        using (var context = new GameAreaMergeTestContext())
        {
            var plan = await context.CreateAsync();
            await SetExecutionStateAsync(context, plan.Id, 1);

            var response = await context.Service.DetailAsync(new() { Id = plan.Id });

            Assert.False(response.Cancellable);
            Assert.True(response.ExecutionReclaimable);
        }
    }

    private static async Task SetExecutionStateAsync(GameAreaMergeTestContext context, long identifier, long executionStartedTime)
    {
        await context.Database.Updateable<TenantGameAreaMergePlanEntity>()
            .SetColumns(item => new TenantGameAreaMergePlanEntity
            {
                Status = GameAreaMergePlanStatus.Executing,
                ExecutionStartedTime = executionStartedTime,
            })
            .Where(item => item.Id == identifier)
            .ExecuteCommandAsync();
    }

    private static async Task<bool> TryCancelAsync(GameAreaMergeTestContext context, long identifier)
    {
        try
        {
            await context.Service.CancelAsync(new() { Id = identifier, Reason = "并发取消" });
            return true;
        }
        catch (BusinessException)
        {
            return false;
        }
    }

    private static async Task<bool> TryExecuteAsync(GameAreaMergeTestContext context, long identifier)
    {
        try
        {
            await context.Service.ExecuteAsync(new()
            {
                Id = identifier, BackupConfirmed = true, BackupIdentifier = "backup-concurrent",
            });
            return true;
        }
        catch (BusinessException)
        {
            return false;
        }
    }
}
