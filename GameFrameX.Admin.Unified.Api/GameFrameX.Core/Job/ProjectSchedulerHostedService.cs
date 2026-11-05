using GameFrameX.Entity.Enum;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameFrameX.Core.Job;

/// <summary>
/// 作业调度宿主服务：注册扫描 / 持久化预载的作业，并在启用持久化的进程内运行触发循环。
/// <para>C404 / GFX-1216 调度合一前本服务只注册不调度（全仓唯一执行入口是任务调度页的手动执行）；
/// 本类补齐触发循环，使内置作业与经扫描派发的业务定时任务统一由本引擎按触发器到期执行。</para>
/// <para>未启用持久化的进程（<c>GFX_DISABLE_SCHEDULE=1</c>）保持既有语义：仅注册占位，不触发。</para>
/// </summary>
public sealed class ProjectSchedulerHostedService : BackgroundService
{
    private const int TriggerLoopTickMilliseconds = 1000;

    private readonly ISchedulerFactory _schedulerFactory;
    private readonly List<IJobPersistence> _persistences;
    private readonly ILogger<ProjectSchedulerHostedService>? _logger;

    public ProjectSchedulerHostedService(ISchedulerFactory schedulerFactory, IEnumerable<IJobPersistence> persistences, ILogger<ProjectSchedulerHostedService>? logger = null)
    {
        _schedulerFactory = schedulerFactory;
        _persistences = persistences.ToList();
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // SqlSugarScope 的 AsyncLocal 槽位可能沿宿主启动链 ExecutionContext 携带共享 client 向下继承
        //（与已退役的 ScheduledTaskHostedService 同源的实测竞态：2026-09-27 docker 栈首查与 Worker 并发流
        // 共享 client，SqlSugar MappingColumns 枚举竞态抛 "Collection was modified" 直接 StopHost）。
        // 整个执行体在 ExecutionContext.SuppressFlow() 下 fork：子流自建独立 client，与启动链隔离。
        using (ExecutionContext.SuppressFlow())
        {
            return Task.Run(() => RunAsync(stoppingToken));
        }
    }

    private async Task RunAsync(CancellationToken stoppingToken)
    {
        foreach (var builder in ProjectTypeCatalog.EffectiveTypes.ScanToBuilders())
        {
            _schedulerFactory.AddJob(builder);
        }

        foreach (var persistence in _persistences)
        {
            var builders = await persistence.PreloadAsync(stoppingToken);
            foreach (var builder in builders)
            {
                _schedulerFactory.AddJob(builder);
            }
        }

        if (_persistences.Count == 0)
        {
            // GFX_DISABLE_SCHEDULE=1：保持既有"只注册占位"语义，不启动触发循环。
            return;
        }

        await RunTriggerLoopAsync(stoppingToken);
    }

    /// <summary>
    /// 触发循环：每秒评估全部作业的全部触发器，到期即触发执行。
    /// <para>评估与状态推进只在循环线程做；执行经 <c>Task.Run</c> 并行发出，单次执行异常不中断循环。</para>
    /// </summary>
    private async Task RunTriggerLoopAsync(CancellationToken stoppingToken)
    {
        _logger?.LogInformation("作业触发循环启动（tick {TickMilliseconds}ms）", TriggerLoopTickMilliseconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                EvaluateAndFire(DateTime.Now);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                // 单轮评估异常不终止循环（与旧引擎"任务执行异常仅记录"的口径一致）。
                _logger?.LogError(exception, "作业触发循环单轮评估异常");
            }

            try
            {
                await Task.Delay(TriggerLoopTickMilliseconds, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // 停机取消：静默退出循环，不让 BackgroundService 以未处理异常收场。
                break;
            }
        }
    }

    private void EvaluateAndFire(DateTime now)
    {
        if (_schedulerFactory is not ProjectSchedulerFactory factory)
        {
            return;
        }

        foreach (var scheduler in factory.GetJobSnapshot())
        {
            if (scheduler.IsPaused() || scheduler.JobType == null)
            {
                continue;
            }

            foreach (var trigger in scheduler.GetTriggerSnapshot())
            {
                EvaluateTrigger(scheduler, trigger, now);
            }
        }
    }

    private void EvaluateTrigger(ProjectScheduler scheduler, Trigger trigger, DateTime now)
    {
        if (trigger.Status == TriggerStatus.Pause || trigger.Status == TriggerStatus.Completed)
        {
            return;
        }

        if (trigger.EndTime != null && now > trigger.EndTime)
        {
            trigger.Status = TriggerStatus.Completed;
            return;
        }

        if (trigger.MaxNumberOfRuns > 0 && trigger.NumberOfRuns >= trigger.MaxNumberOfRuns)
        {
            trigger.Status = TriggerStatus.Completed;
            return;
        }

        if (trigger.NextRunTime == null)
        {
            if (trigger.RunOnStart && trigger.NumberOfRuns == 0)
            {
                Fire(scheduler, trigger, now);
                return;
            }

            var initialNext = TriggerSchedule.GetNextOccurrence(trigger, now);
            if (trigger.StartTime != null && trigger.StartTime > now)
            {
                initialNext = trigger.StartTime;
            }

            trigger.NextRunTime = initialNext;
            if (initialNext == null)
            {
                // 解释不了的触发参数：置 Completed 退出评估（避免每秒重复告警）；
                // 任务调度 UI 修正 Args 后 UpdateTriggerAsync 会以 Ready 状态重建触发器，自动恢复。
                trigger.Status = TriggerStatus.Completed;
                WarnDormantTrigger(scheduler, trigger);
            }

            return;
        }


        if (trigger.NextRunTime <= now)
        {
            Fire(scheduler, trigger, now);
        }
    }

    private void Fire(ProjectScheduler scheduler, Trigger trigger, DateTime now)
    {
        var jobId = scheduler.JobId ?? trigger.JobId ?? trigger.TriggerId;

        // Concurrent=false 且上一次执行未结束：跳过本次触发（错过即错过，不排队），推进下一次时间。
        if (!scheduler.TryBeginExecution())
        {
            trigger.NextRunTime = TriggerSchedule.GetNextOccurrence(trigger, now);
            _logger?.LogWarning("作业 {JobId}/{TriggerId} 上一次执行未结束，跳过本次触发", jobId, trigger.TriggerId);
            return;
        }

        trigger.NumberOfRuns++;
        trigger.LastRunTime = now;
        trigger.NextRunTime = TriggerSchedule.GetNextOccurrence(trigger, now);
        if (trigger.MaxNumberOfRuns > 0 && trigger.NumberOfRuns >= trigger.MaxNumberOfRuns)
        {
            trigger.Status = TriggerStatus.Completed;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await scheduler.RunAsync(CancellationToken.None);
            }
            catch (Exception exception)
            {
                trigger.NumberOfErrors++;
                _logger?.LogError(exception, "作业 {JobId}/{TriggerId} 执行异常", jobId, trigger.TriggerId);
            }
            finally
            {
                scheduler.EndExecution();
            }
        });
    }

    /// <summary>
    /// 解释不了的触发器（类型未知 / 参数缺失）：告警一次并置休眠，不参与后续评估。
    /// </summary>
    private void WarnDormantTrigger(ProjectScheduler scheduler, Trigger trigger)
    {
        _logger?.LogWarning("触发器 {JobId}/{TriggerId}（TriggerType={TriggerType}, Args={Arguments}）无法解释调度参数，保持休眠",
                            scheduler.JobId, trigger.TriggerId, trigger.TriggerType, trigger.Args);
    }
}
