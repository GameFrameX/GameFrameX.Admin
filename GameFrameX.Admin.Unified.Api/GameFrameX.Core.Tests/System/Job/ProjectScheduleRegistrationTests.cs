using GameFrameX.Application.Platform.Job;
using GameFrameX.Core.Job;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GameFrameX.Core.Tests.System.Job;

/// <summary>
/// 调度注册与内置作业声明绑定测试：
/// 双入口（AdminScheduleSetup + GameScheduleSetup）注册不重复；触发器特性的调度参数绑定进 Args；
/// 旧引擎 ScheduledTaskHostedService 已退役。
/// </summary>
public sealed class ProjectScheduleRegistrationTests
{
    [Fact]
    public void AddSchedule_DualEntryPoints_RegisterSingleFactoryHostedServiceAndPersistence()
    {
        // 与 Startup 等价的双入口调用序列。
        var services = new ServiceCollection();
        services.AddSchedule(options => options.AddPersistence<NoOperationJobPersistence>());
        services.AddSchedule();

        var provider = services.BuildServiceProvider();

        Assert.Single(provider.GetServices<ISchedulerFactory>());
        Assert.Single(provider.GetServices<IHostedService>().OfType<ProjectSchedulerHostedService>());
        Assert.Single(provider.GetServices<IJobPersistence>().OfType<NoOperationJobPersistence>());
    }

    [Fact]
    public void ScanToBuilder_PeriodSecondsAttribute_BindsSecondsIntoArguments()
    {
        var builder = typeof(GameHomeOnlineSampleJob).ScanToBuilder();

        var trigger = builder.GetEnumerable().Single();
        Assert.Equal(TriggerSchedule.PeriodTriggerType, trigger.Trigger.TriggerType, StringComparer.OrdinalIgnoreCase);
        Assert.Equal(TriggerSchedule.PeriodArguments(900), trigger.Trigger.Args);
    }

    [Fact]
    public void ScanToBuilder_CronAttribute_BindsExpressionIntoArguments()
    {
        var builder = typeof(SampleCronJob).ScanToBuilder();

        var trigger = builder.GetEnumerable().Single();
        Assert.Equal(TriggerSchedule.CronTriggerType, trigger.Trigger.TriggerType, StringComparer.OrdinalIgnoreCase);
        Assert.Equal(TriggerSchedule.CronArguments("0 30 3 * * *"), trigger.Trigger.Args);
    }

    [Fact]
    public void ScheduledTaskHostedService_IsRetiredFromApplicationAssembly()
    {
        // 调度合一：旧引擎类型必须不存在于任何产线程序集，Startup 亦不再注册（类型删除即编译期保证）。
        var assembly = typeof(GameFrameX.Application.Platform.Job.GameTimeJobDispatchJob).Assembly;
        Assert.Null(assembly.GetType("GameFrameX.Application.Platform.Job.ScheduledTaskHostedService", false));
    }

    /// <summary>
    /// 验证 [Cron] 特性声明的内置作业（为 R3 审计留存的每天 03:30 形态提供样板）。
    /// </summary>
    [JobDetail("job_sampleCronForTest", Description = "测试用 cron 作业", GroupName = "default")]
    [Cron("0 30 3 * * *", TriggerId = "trigger_sampleCronForTest", Description = "每天 03:30")]
    private sealed class SampleCronJob : IJob
    {
        public Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
        {
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

        public Task PersistRegisteredAsync(JobDetail jobDetail, IReadOnlyList<Trigger> triggers, CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
