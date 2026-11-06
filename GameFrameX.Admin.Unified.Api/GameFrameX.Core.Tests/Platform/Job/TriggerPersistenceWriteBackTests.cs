using GameFrameX.Application.Audit.Retention;
using GameFrameX.Application.Platform.Job;
using GameFrameX.Core.Job;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

/// <summary>
/// C404 §9.8：注册期持久化（内置作业 upsert 进 SystemJob 表，任务调度页可见）
/// 与触发器运行态写回（NumberOfRuns / LastRunTime 落库）。
/// </summary>
public sealed class TriggerPersistenceWriteBackTests : IDisposable
{
    private readonly ClientApiTestDb _database = new();
    private readonly ServiceProvider _provider;

    public TriggerPersistenceWriteBackTests()
    {
        _database.InitTenantTables(typeof(SystemJobDetailEntity), typeof(SystemJobTriggerEntity));

        var services = new ServiceCollection();
        services.AddSingleton<ISqlSugarClient>(_database.MainDb);
        services.AddScoped(typeof(SqlSugarRepository<>));
        services.AddSingleton<IJobPersistence, DbJobPersistence>();
        _provider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _provider.Dispose();
        _database.Dispose();
    }

    [Fact]
    public async Task PersistRegisteredAsync_InsertsBuiltInJobAndTrigger_ThenIdempotentUpdate()
    {
        var persistence = _provider.GetRequiredService<IJobPersistence>();
        var builder = typeof(AuditRetentionJob).ScanToBuilder();
        var detail = builder.GetJobBuilder().Detail;
        var triggers = builder.GetEnumerable().Select(item => item.Trigger).ToArray();

        await persistence.PersistRegisteredAsync(detail, triggers, CancellationToken.None);
        await persistence.PersistRegisteredAsync(detail, triggers, CancellationToken.None);

        var detailRow = _database.MainDb.Queryable<SystemJobDetailEntity>().Single(u => u.JobId == "job_auditRetention");
        Assert.Equal("审计日志归档", detailRow.Description);
        var triggerRow = _database.MainDb.Queryable<SystemJobTriggerEntity>().Single(u => u.JobId == "job_auditRetention");
        Assert.Equal(TriggerSchedule.CronTriggerType, triggerRow.TriggerType, StringComparer.OrdinalIgnoreCase);
        Assert.Equal(TriggerSchedule.CronArguments("0 30 3 * * *"), triggerRow.Args);
    }

    [Fact]
    public async Task PersistTriggerRuntimeAsync_WritesBackRunState()
    {
        var persistence = (DbJobPersistence)_provider.GetServices<IJobPersistence>().Single();
        var builder = typeof(AuditRetentionJob).ScanToBuilder();

        var scheduler = new ProjectScheduler(builder.GetJobBuilder(), _provider, _provider.GetServices<IJobPersistence>());
        var triggerBuilder = builder.GetEnumerable().Single();
        scheduler.AttachTriggerBuilder(triggerBuilder);
        await persistence.PersistRegisteredAsync(builder.GetJobBuilder().Detail, scheduler.GetTriggerSnapshot(), CancellationToken.None);

        var trigger = scheduler.GetTriggerSnapshot().Single();
        trigger.NumberOfRuns = 3;
        trigger.LastRunTime = new DateTime(2026, 11, 5, 3, 30, 0);
        trigger.NextRunTime = new DateTime(2026, 11, 6, 3, 30, 0);
        await scheduler.PersistTriggerRuntimeAsync(trigger);

        var row = _database.MainDb.Queryable<SystemJobTriggerEntity>().Single(u => u.JobId == "job_auditRetention");
        Assert.Equal(3, row.NumberOfRuns);
        Assert.Equal(new DateTime(2026, 11, 5, 3, 30, 0, DateTimeKind.Utc), ScheduleTimeBridge.FromUnixSeconds(row.LastRunTime));
        Assert.Equal(new DateTime(2026, 11, 6, 3, 30, 0, DateTimeKind.Utc), ScheduleTimeBridge.FromUnixSeconds(row.NextRunTime));
        Assert.Equal(TriggerStatus.Ready, row.Status);
    }

    [Fact]
    public void ScanToBuilder_AuditRetentionJob_BindsDailyCronExpression()
    {
        var trigger = typeof(AuditRetentionJob).ScanToBuilder().GetEnumerable().Single().Trigger;

        Assert.Equal(TriggerSchedule.CronTriggerType, trigger.TriggerType, StringComparer.OrdinalIgnoreCase);
        Assert.Equal(TriggerSchedule.CronArguments("0 30 3 * * *"), trigger.Args);
        // 每天 03:30：从任意时刻起算，下一次要么是当天 03:30 要么是次日 03:30。
        var noon = new DateTime(2026, 11, 5, 12, 0, 0);
        var next = TriggerSchedule.GetNextOccurrence(trigger, noon);
        Assert.Equal(new DateTime(2026, 11, 6, 3, 30, 0), next);
    }
}
