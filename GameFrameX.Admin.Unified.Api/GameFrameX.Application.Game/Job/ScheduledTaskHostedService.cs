using GameFrameX.Application.Game.TimeJob.GameTimeJob.Dto;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;

namespace GameFrameX.Application.Game.Job;

public class ScheduledTaskHostedService : BackgroundService
{
    private readonly ILogger<ScheduledTaskHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly List<ScheduledTask> _scheduledTasks = new();
    private readonly SemaphoreSlim _tasksLock = new(1, 1);
    private readonly SemaphoreSlim _scheduleChanged = new(0, int.MaxValue);
    private readonly SystemCacheService _systemCacheService;

    public ScheduledTaskHostedService(ILogger<ScheduledTaskHostedService> logger, IServiceProvider serviceProvider, SystemCacheService systemCacheService)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _systemCacheService = systemCacheService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // GFX-872 / C380：与 TenantMenuSyncWorker 同根修复——SqlSugarScope 的 AsyncLocal 槽位可能沿宿主启动链
        // ExecutionContext 携带共享 client 向下继承（docker 栈 2026-09-27 17:04:07 实测：本服务首查与 Worker
        // 并发流共享同一 client，SqlSugar 内部 MappingColumns 列表枚举竞态抛 "Collection was modified"，
        // 未捕获直接 StopHost → Hosting failed to start）。整个执行体在 ExecutionContext.SuppressFlow() 下
        // fork：子流从默认上下文起步，SqlSugar 槽位为空、自建独立 client，与启动链共享槽位隔离。
        using (ExecutionContext.SuppressFlow())
        {
            return Task.Run(() => RunAsync(stoppingToken));
        }
    }

    private async Task RunAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ScheduledTaskHostedService 启动");

        await LoadScheduledTasksAsync(stoppingToken);
        await RunScheduleLoopAsync(stoppingToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ScheduledTaskHostedService 停止");

        return base.StopAsync(cancellationToken);
    }

    private async Task LoadScheduledTasksAsync(CancellationToken cancellationToken)
    {
        // 从每个启用租户库拿到定时任务配置，禁止在无租户上下文时回落主库查询 tenant_ 表。
        // Output 已拆分为纯 POCO（不含 TenantId），用配对记录携带租户 Id 供 JobConfig 映射。
        var jobEntityOutputs = new List<TenantJobEntry>();
        using (var scope = _serviceProvider.CreateScope())
        {
            var tenantRepository = scope.ServiceProvider.GetRequiredService<SqlSugarRepository<SystemTenantEntity>>();
            var tenantDbConnectionProvider = scope.ServiceProvider.GetRequiredService<TenantDbConnectionProvider>();
            var tenants = await tenantRepository.AsQueryable()
                                         .Filter(null, true)
                                         .Where(u => u.IsEnabled == true && u.TenantType == TenantTypeEnum.Db && !string.IsNullOrWhiteSpace(u.Connection))
                                         .ToListAsync(cancellationToken);

            foreach (var tenant in tenants)
            {
                var tenantDb = tenantDbConnectionProvider.GetTenantDbConnectionScope(tenant.Id);
                if (tenantDb == null)
                {
                    _logger.LogWarning("租户 {TenantId} 未取得租户库连接，已跳过定时任务加载。", tenant.Id);
                    continue;
                }

                using (TenantOperationContext.UseTenant(tenant.Id))
                {
                    var tenantJobs = await tenantDb.Queryable<TenantGameTimeJobEntity>()
                                                   .Where(u => u.TenantId == tenant.Id)
                                                   .Select<TenantGameTimeJobEntityResponse>()
                                                   .ToListAsync(cancellationToken);
                    foreach (var job in tenantJobs)
                    {
                        jobEntityOutputs.Add(new TenantJobEntry { TenantId = tenant.Id, Job = job, });
                    }
                }
            }
        }

        var jobConfigs = new List<JobConfig>();
        jobEntityOutputs.ForEach(item =>
        {
            var config = new JobConfig
            {
                TenantId = item.TenantId,
                JobType = item.Job.Name,
                CronExpression = item.Job.Cron,
                Description = item.Job.Description,
                IsOnce = item.Job.IsOnce,
                MailId = item.Job.MailId > 0 ? item.Job.MailId : 0,
                Id = item.Job.Id,
            };
            jobConfigs.Add(config);
        });

        if (jobConfigs.Count == 0)
        {
            _logger.LogWarning("未找到任何定时任务配置。");
            return;
        }

        foreach (var jobConfig in jobConfigs)
        {
            await AddTaskAsync(jobConfig, cancellationToken);
        }
    }

    /// <summary>
    /// 添加任务
    /// </summary>
    /// <param name="jobConfig"></param>
    public Task AddTaskAsync(JobConfig jobConfig)
    {
        return AddTaskAsync(jobConfig, CancellationToken.None);
    }

    private async Task AddTaskAsync(JobConfig jobConfig, CancellationToken cancellationToken)
    {
        var tenantId = jobConfig.TenantId > 0 ? jobConfig.TenantId : TenantOperationContext.GetEffectiveTenantId();
        if (!tenantId.HasValue)
        {
            _logger.LogWarning("定时任务 {JobType} 缺少租户上下文，已跳过注册。", jobConfig.JobType);
            return;
        }

        // 确保 JobType 包含完整命名空间
        var jobType = Type.GetType($"GameFrameX.Application.Game.Job.{jobConfig.JobType}");
        if (jobType == null)
        {
            _logger.LogWarning($"未找到任务类型: {jobConfig.JobType}");
            return;
        }

        // 确保 jobType 实现了 IScheduleJob 接口
        if (!typeof(IScheduleJob).IsAssignableFrom(jobType))
        {
            _logger.LogWarning($"任务类型 {jobConfig.JobType} 未实现 IScheduleJob 接口");
            return;
        }

        // 解析 Cron 表达式
        CrontabSchedule schedule;
        try
        {
            schedule = CrontabSchedule.Parse(jobConfig.CronExpression, new CrontabSchedule.ParseOptions { IncludingSeconds = true, });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"解析 Cron 表达式失败: {jobConfig.CronExpression}");
            return;
        }

        var nextRun = schedule.GetNextOccurrence(DateTime.Now);

        var scheduledTask = new ScheduledTask
        {
            TenantId = tenantId.Value,
            JobType = jobConfig.JobType,
            CronExpression = jobConfig.CronExpression,
            Description = jobConfig.Description,
            NextRun = nextRun,
            IsOnce = jobConfig.IsOnce,
            MailId = jobConfig.MailId,
            Id = jobConfig.Id,
        };

        await _tasksLock.WaitAsync(cancellationToken);
        try
        {
            _scheduledTasks.Add(scheduledTask);
        }
        finally
        {
            _tasksLock.Release();
        }

        _logger.LogInformation($"已注册任务: {jobConfig.Description}, 下一次执行时间: {nextRun}");
        SignalScheduleChanged();
    }

    private async Task RunScheduleLoopAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = await GetDelayToNextRunAsync(stoppingToken);
            if (delay == null)
            {
                await WaitForScheduleChangedAsync(Timeout.InfiniteTimeSpan, stoppingToken);
                continue;
            }

            await WaitForScheduleChangedAsync(delay.Value, stoppingToken);
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            await ExecuteDueTasksAsync(stoppingToken);
        }
    }

    private async Task<TimeSpan?> GetDelayToNextRunAsync(CancellationToken cancellationToken)
    {
        await _tasksLock.WaitAsync(cancellationToken);
        try
        {
            if (_scheduledTasks.Count == 0)
            {
                _logger.LogWarning("没有需要调度的任务。");
                return null;
            }

            var now = DateTime.Now;
            var nextRun = _scheduledTasks.Min(t => t.NextRun);
            var dueTime = nextRun - now;
            if (dueTime.TotalMilliseconds <= 0)
            {
                dueTime = TimeSpan.Zero;
            }

            _logger.LogInformation($"下次任务将在 {nextRun} 执行 (延迟 {dueTime.TotalSeconds} 秒)");

            return dueTime;
        }
        finally
        {
            _tasksLock.Release();
        }
    }

    private async Task WaitForScheduleChangedAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        try
        {
            await _scheduleChanged.WaitAsync(timeout, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private void SignalScheduleChanged()
    {
        if (_scheduleChanged.CurrentCount == 0)
        {
            _scheduleChanged.Release();
        }
    }

    private async Task ExecuteDueTasksAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"触发执行任务时间: {DateTime.Now}");

        List<ScheduledTask> dueTasks;
        await _tasksLock.WaitAsync(cancellationToken);
        try
        {
            var now = DateTime.Now;
            dueTasks = _scheduledTasks.Where(t => t.NextRun <= now).ToList();
        }
        finally
        {
            _tasksLock.Release();
        }

        if (dueTasks.Count == 0)
        {
            return;
        }

        await Task.WhenAll(dueTasks.Select(task => ExecuteTaskAsync(task, cancellationToken)));
    }

    private async Task ExecuteTaskAsync(ScheduledTask scheduledTask, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation($"开始执行任务: {scheduledTask.Description}");

            using var tenantContext = TenantOperationContext.UseTenant(scheduledTask.TenantId);
            using var scope = _serviceProvider.CreateScope();
#pragma warning disable CS8604 // Type.GetType 未命中返回 null；GetService(Type) 接受 null 并返回 null（即未注册），符合 DI 容器语义
            var job = scope.ServiceProvider.GetService(Type.GetType($"GameFrameX.Application.Game.Job.{scheduledTask.JobType}")) as IScheduleJob;
#pragma warning restore CS8604
            if (job == null)
            {
                _logger.LogWarning($"无法找到任务类型: {scheduledTask.JobType}");
                return;
            }

            _systemCacheService.Set("MailId", scheduledTask.MailId);

            var innerTask = await job.ExecuteAsync(cancellationToken);
            if (innerTask != null)
            {
                await innerTask;
            }

            // 计算下一个执行时间
            if (!scheduledTask.IsOnce)
            {
                var schedule = CrontabSchedule.Parse(scheduledTask.CronExpression, new CrontabSchedule.ParseOptions { IncludingSeconds = true, });
                await _tasksLock.WaitAsync(cancellationToken);
                try
                {
                    scheduledTask.NextRun = schedule.GetNextOccurrence(DateTime.Now);
                }
                finally
                {
                    _tasksLock.Release();
                }
            }
            else
            {
                var repository = scope.ServiceProvider.GetRequiredService<SqlSugarRepository<TenantGameTimeJobEntity>>();
                if (scheduledTask.Id > 0)
                {
                    await repository.DeleteByIdAsync(scheduledTask.Id);
                }

                await _tasksLock.WaitAsync(cancellationToken);
                try
                {
                    _scheduledTasks.Remove(scheduledTask);
                }
                finally
                {
                    _tasksLock.Release();
                }
            }

            _logger.LogInformation($"完成执行任务: {scheduledTask.Description}, 下一次执行时间: {scheduledTask.NextRun}");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation($"任务 {scheduledTask.Description} 已取消");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"执行任务 {scheduledTask.Description} 时发生异常");
        }
    }

    public override void Dispose()
    {
        _tasksLock.Dispose();
        _scheduleChanged.Dispose();
        base.Dispose();
    }

    // 定义 ScheduledTask 类
    private class ScheduledTask
    {
        public string JobType { get; set; } = null!;
        public string CronExpression { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime NextRun { get; set; }
        public bool IsOnce { get; set; }
        public long MailId { get; set; }
        public long Id { get; set; }
        public long TenantId { get; set; }
    }

    /// <summary>
    /// 租户 Id 与其定时任务 Output 的配对记录（Output 纯 POCO 不含 TenantId，内部编排需要租户归属）。
    /// </summary>
    private sealed class TenantJobEntry
    {
        public long TenantId { get; set; }

        public TenantGameTimeJobEntityResponse Job { get; set; } = null!;
    }
}