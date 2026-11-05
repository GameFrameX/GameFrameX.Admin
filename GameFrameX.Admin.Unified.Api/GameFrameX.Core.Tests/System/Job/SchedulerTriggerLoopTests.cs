using GameFrameX.Core.Job;
using GameFrameX.Entity.Enum;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace GameFrameX.Core.Tests.System.Job;

/// <summary>
/// 作业触发循环（ProjectSchedulerHostedService）集成测试：周期触发器到期执行、
/// RunOnStart 立即执行、MaxNumberOfRuns 到量停跑、未启用持久化不启动循环。
/// <para>探针作业以内置作业注册方式（[JobDetail] + [PeriodSeconds] 特性）声明，
/// 由宿主服务的类型扫描注册（测试程序集名为 GameFrameX.* 同样被 ProjectTypeCatalog 收录），
/// 同时端到端验证"特性 → Args 绑定 → 触发循环执行"链路。</para>
/// </summary>
public sealed class SchedulerTriggerLoopTests
{
    [Fact]
    public async Task TriggerLoop_PeriodTrigger_FiresRepeatedly()
    {
        CountingPeriodicJob.Reset();

        await RunLoopUntilAsync(() => CountingPeriodicJob.ExecutionCount >= 2, includePersistence: true);

        Assert.True(CountingPeriodicJob.ExecutionCount >= 2, $"周期触发器应在等待窗口内至少执行两次，实际 {CountingPeriodicJob.ExecutionCount} 次");
    }

    [Fact]
    public async Task TriggerLoop_RunOnStart_FiresImmediatelyWithoutWaitingNextOccurrence()
    {
        CountingRunOnStartJob.Reset();

        await RunLoopUntilAsync(() => CountingRunOnStartJob.ExecutionCount >= 1, includePersistence: true);
        // RunOnStart 已触发一次，下一次在 3600 秒后：多等一个 tick 确认不会再次执行。
        await Task.Delay(TimeSpan.FromSeconds(1.5));

        Assert.Equal(1, CountingRunOnStartJob.ExecutionCount);
    }

    [Fact]
    public async Task TriggerLoop_MaxNumberOfRunsReached_TriggerCompletes()
    {
        CountingMaxRunsJob.Reset();

        var resolvedFactory = await RunLoopUntilAsync(() => CountingMaxRunsJob.ExecutionCount >= 1, includePersistence: true);
        // 到量后再等两个周期，确认不再触发。
        await Task.Delay(TimeSpan.FromSeconds(2.5));

        Assert.Equal(1, CountingMaxRunsJob.ExecutionCount);
        var trigger = resolvedFactory.GetJob(CountingMaxRunsJob.JobIdentity)!.GetTriggerSnapshot().Single();
        Assert.Equal(TriggerStatus.Completed, trigger.Status);
    }

    [Fact]
    public async Task TriggerLoop_WithoutPersistence_DoesNotFire()
    {
        CountingPeriodicJob.Reset();

        var provider = BuildProvider(includePersistence: false);
        using (var cancellation = new CancellationTokenSource())
        {
            var hostedService = new ProjectSchedulerHostedService(provider.GetRequiredService<ISchedulerFactory>(),
                                                                  provider.GetServices<IJobPersistence>(),
                                                                  NullLogger<ProjectSchedulerHostedService>.Instance);
            await hostedService.StartAsync(cancellation.Token);

            await Task.Delay(TimeSpan.FromSeconds(2.5));
            await cancellation.CancelAsync();
            await hostedService.StopAsync(CancellationToken.None);
        }

        // GFX_DISABLE_SCHEDULE=1 语义：未启用持久化的进程只注册占位，不启动触发循环。
        Assert.Equal(0, CountingPeriodicJob.ExecutionCount);
    }

    private static async Task<ProjectSchedulerFactory> RunLoopUntilAsync(Func<bool> condition, bool includePersistence)
    {
        var provider = BuildProvider(includePersistence);
        var cancellation = new CancellationTokenSource();
        var hostedService = new ProjectSchedulerHostedService(provider.GetRequiredService<ISchedulerFactory>(),
                                                              provider.GetServices<IJobPersistence>(),
                                                              NullLogger<ProjectSchedulerHostedService>.Instance);
        await hostedService.StartAsync(cancellation.Token);

        try
        {
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);
            while (DateTime.UtcNow < deadline)
            {
                if (condition())
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
        }
        finally
        {
            await cancellation.CancelAsync();
            await hostedService.StopAsync(CancellationToken.None);
        }

        return (ProjectSchedulerFactory)provider.GetRequiredService<ISchedulerFactory>();
    }

    private static IServiceProvider BuildProvider(bool includePersistence)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISchedulerFactory, ProjectSchedulerFactory>();
        if (includePersistence)
        {
            services.AddSingleton<IJobPersistence, NoOperationJobPersistence>();
        }

        return services.BuildServiceProvider();
    }

    [JobDetail("job_countingPeriodicForLoopTest", Description = "循环测试周期探针", GroupName = "default")]
    [PeriodSeconds(1, TriggerId = "trigger_countingPeriodicForLoopTest", Description = "循环测试周期探针")]
    private sealed class CountingPeriodicJob : IJob
    {
        public static int ExecutionCount;

        public static void Reset()
        {
            ExecutionCount = 0;
        }

        public Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
        {
            ExecutionCount++;
            return Task.CompletedTask;
        }
    }

    [JobDetail("job_countingRunOnStartForLoopTest", Description = "循环测试启动即跑探针", GroupName = "default")]
    [PeriodSeconds(3600, TriggerId = "trigger_countingRunOnStartForLoopTest", Description = "循环测试启动即跑探针", RunOnStart = true)]
    private sealed class CountingRunOnStartJob : IJob
    {
        public static int ExecutionCount;

        public static void Reset()
        {
            ExecutionCount = 0;
        }

        public Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
        {
            ExecutionCount++;
            return Task.CompletedTask;
        }
    }

    [JobDetail("job_countingMaxRunsForLoopTest", Description = "循环测试限次探针", GroupName = "default")]
    [PeriodSeconds(1, TriggerId = "trigger_countingMaxRunsForLoopTest", Description = "循环测试限次探针", MaxNumberOfRuns = 1)]
    private sealed class CountingMaxRunsJob : IJob
    {
        public const string JobIdentity = "job_countingMaxRunsForLoopTest";

        public static int ExecutionCount;

        public static void Reset()
        {
            ExecutionCount = 0;
        }

        public Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
        {
            ExecutionCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class NoOperationJobPersistence : IJobPersistence
    {
        public Task<IEnumerable<SchedulerBuilder>> PreloadAsync(CancellationToken stoppingToken)
        {
            return Task.FromResult<IEnumerable<SchedulerBuilder>>(Array.Empty<SchedulerBuilder>());
        }

        public Task<SchedulerBuilder> OnLoadingAsync(SchedulerBuilder builder, CancellationToken stoppingToken)
        {
            return Task.FromResult(builder);
        }

        public Task OnChangedAsync(PersistenceContext context)
        {
            return Task.CompletedTask;
        }

        public Task OnTriggerChangedAsync(PersistenceTriggerContext context)
        {
            return Task.CompletedTask;
        }

        public Task OnExecutionRecordAsync(PersistenceExecutionRecordContext context)
        {
            return Task.CompletedTask;
        }
    }
}
