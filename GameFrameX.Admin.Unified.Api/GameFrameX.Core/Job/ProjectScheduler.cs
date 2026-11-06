using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Job;

public sealed class ProjectScheduler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly List<IJobPersistence> _persistences;
    private readonly List<TriggerBuilder> _triggerBuilders = new();
    private readonly object _triggerLock = new();
    private JobBuilder _jobBuilder;
    private bool _paused;
    private int _executing;

    public ProjectScheduler(JobBuilder jobBuilder, IServiceProvider serviceProvider, IEnumerable<IJobPersistence> persistences)
    {
        _jobBuilder = jobBuilder;
        _serviceProvider = serviceProvider;
        _persistences = persistences.ToList();
    }

    public string? JobId
    {
        get { return _jobBuilder.JobId; }
    }

    /// <summary>
    /// 作业 CLR 类型（无法解析时为 null，触发循环跳过）。
    /// </summary>
    internal Type? JobType
    {
        get { return _jobBuilder.JobType; }
    }

    /// <summary>
    /// 触发器运行态写回（C404 §9.8）：执行完成后把 NumberOfRuns / LastRunTime / NextRunTime /
    /// Status / NumberOfErrors 经既有持久化链（OnTriggerChangedAsync，Updated）落库，
    /// 供「任务调度」页展示真实运行态。无持久化或行不存在时静默无效果（UPDATE 命中 0 行）。
    /// </summary>
    internal async Task PersistTriggerRuntimeAsync(Trigger trigger)
    {
        trigger.UpdatedTime = DateTime.Now;
        await NotifyTriggerChangedAsync(trigger, PersistenceBehavior.Updated);
    }

    public JobBuilder GetJobBuilder()
    {
        return _jobBuilder;
    }

    public Task UpdateDetailAsync(JobBuilder builder)
    {
        _jobBuilder = builder;
        return NotifyChangedAsync(PersistenceBehavior.Updated);
    }

    public Task AddTriggerAsync(TriggerBuilder builder)
    {
        builder.Trigger.JobId = JobId;
        lock (_triggerLock)
        {
            var existingIndex = _triggerBuilders.FindIndex(item => string.Equals(item.TriggerId, builder.TriggerId, StringComparison.Ordinal));
            if (existingIndex >= 0)
            {
                _triggerBuilders[existingIndex] = builder;
            }
            else
            {
                _triggerBuilders.Add(builder);
            }
        }

        return NotifyTriggerChangedAsync(builder.Trigger, PersistenceBehavior.Appended);
    }

    public Task UpdateTriggerAsync(TriggerBuilder builder)
    {
        builder.Trigger.JobId = JobId;
        lock (_triggerLock)
        {
            var existingIndex = _triggerBuilders.FindIndex(item => string.Equals(item.TriggerId, builder.TriggerId, StringComparison.Ordinal));
            if (existingIndex >= 0)
            {
                _triggerBuilders[existingIndex] = builder;
            }
            else
            {
                _triggerBuilders.Add(builder);
            }
        }

        return NotifyTriggerChangedAsync(builder.Trigger, PersistenceBehavior.Updated);
    }

    public Task RemoveTriggerAsync(string triggerId)
    {
        lock (_triggerLock)
        {
            _triggerBuilders.RemoveAll(item => string.Equals(item.TriggerId, triggerId, StringComparison.Ordinal));
        }

        return NotifyTriggerChangedAsync(new Trigger { JobId = JobId, TriggerId = triggerId, }, PersistenceBehavior.Removed);
    }

    public void Pause()
    {
        _paused = true;
    }

    public void Start()
    {
        _paused = false;
    }

    public void PauseTrigger(string triggerId)
    {
        lock (_triggerLock)
        {
            var trigger = _triggerBuilders.FirstOrDefault(item => string.Equals(item.TriggerId, triggerId, StringComparison.Ordinal))?.Trigger;
            if (trigger != null)
            {
                trigger.Status = TriggerStatus.Pause;
            }
        }
    }

    public void StartTrigger(string triggerId)
    {
        lock (_triggerLock)
        {
            var trigger = _triggerBuilders.FirstOrDefault(item => string.Equals(item.TriggerId, triggerId, StringComparison.Ordinal))?.Trigger;
            if (trigger != null)
            {
                trigger.Status = TriggerStatus.Ready;
            }
        }
    }

    public async Task<object?> RunAsync(CancellationToken cancellationToken)
    {
        if (_paused || _jobBuilder.JobType == null)
        {
            return null;
        }

        using var scope = _serviceProvider.CreateScope();
        var job = (IJob)ActivatorUtilities.CreateInstance(scope.ServiceProvider, _jobBuilder.JobType);
        var context = new JobExecutingContext { JobDetail = _jobBuilder.Detail, };
        await job.ExecuteAsync(context, cancellationToken);
        return context.Result;
    }

    /// <summary>
    /// 挂载扫描 / 预载得到的初始触发器（仅内存，不通知持久化——持久化由 Preload 语义负责）。
    /// </summary>
    internal void AttachTriggerBuilder(TriggerBuilder triggerBuilder)
    {
        triggerBuilder.Trigger.JobId = JobId;
        lock (_triggerLock)
        {
            if (_triggerBuilders.All(item => !string.Equals(item.TriggerId, triggerBuilder.TriggerId, StringComparison.Ordinal)))
            {
                _triggerBuilders.Add(triggerBuilder);
            }
        }
    }

    /// <summary>
    /// 当前触发器快照（触发循环逐个评估用）。
    /// </summary>
    internal IReadOnlyList<Trigger> GetTriggerSnapshot()
    {
        lock (_triggerLock)
        {
            return _triggerBuilders.Select(item => item.Trigger).ToArray();
        }
    }

    /// <summary>
    /// 是否暂停调度（触发循环跳过本作业）。
    /// </summary>
    internal bool IsPaused()
    {
        return _paused;
    }

    /// <summary>
    /// 尝试占用执行位：<see cref="JobDetail.Concurrent"/> 为 false 时上一次执行未结束则拒绝（本次触发跳过，不排队）。
    /// </summary>
    internal bool TryBeginExecution()
    {
        if (!_jobBuilder.Detail.Concurrent)
        {
            return Interlocked.CompareExchange(ref _executing, 1, 0) == 0;
        }

        return true;
    }

    /// <summary>
    /// 释放执行位（与 <see cref="TryBeginExecution"/> 配对）。
    /// </summary>
    internal void EndExecution()
    {
        Interlocked.Exchange(ref _executing, 0);
    }

    private async Task NotifyChangedAsync(PersistenceBehavior behavior)
    {
        var context = new PersistenceContext(_jobBuilder.Detail, behavior);
        foreach (var persistence in _persistences)
        {
            await persistence.OnChangedAsync(context);
        }
    }

    private async Task NotifyTriggerChangedAsync(Trigger trigger, PersistenceBehavior behavior)
    {
        var context = new PersistenceTriggerContext(trigger, behavior);
        foreach (var persistence in _persistences)
        {
            await persistence.OnTriggerChangedAsync(context);
        }
    }
}