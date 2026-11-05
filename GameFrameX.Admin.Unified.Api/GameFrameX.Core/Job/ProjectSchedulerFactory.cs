namespace GameFrameX.Core.Job;

public sealed class ProjectSchedulerFactory : ISchedulerFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<IJobPersistence> _persistences;
    private readonly ConcurrentDictionary<string, ProjectScheduler> _jobs = new(StringComparer.Ordinal);
    private volatile bool _paused;

    public ProjectSchedulerFactory(IServiceProvider serviceProvider, IEnumerable<IJobPersistence> persistences)
    {
        _serviceProvider = serviceProvider;
        _persistences = persistences;
    }

    public void AddJob(JobBuilder builder)
    {
        AddJob(SchedulerBuilder.Create(builder));
    }

    /// <summary>
    /// 注册作业并保留其触发器（触发循环按触发器评估执行；JobBuilder-only 重载不携带触发器）。
    /// </summary>
    public void AddJob(SchedulerBuilder builder)
    {
        var jobBuilder = builder.GetJobBuilder();
        if (string.IsNullOrWhiteSpace(jobBuilder.JobId))
        {
            return;
        }

        var scheduler = new ProjectScheduler(jobBuilder, _serviceProvider, _persistences);
        foreach (var triggerBuilder in builder.GetEnumerable())
        {
            scheduler.AttachTriggerBuilder(triggerBuilder);
        }

        _jobs[jobBuilder.JobId] = scheduler;
    }

    public ProjectScheduler? GetJob(string jobId)
    {
        return _jobs.GetValueOrDefault(jobId);
    }

    public void RemoveJob(string jobId)
    {
        _jobs.TryRemove(jobId, out _);
    }

    /// <summary>
    /// 当前作业快照（触发循环评估用）。
    /// </summary>
    public IReadOnlyCollection<ProjectScheduler> GetJobSnapshot()
    {
        return _jobs.Values.ToArray();
    }

    public void PauseAll()
    {
        _paused = true;
    }

    public void StartAll()
    {
        _paused = false;
    }

    public async Task<ScheduleResult> TryRunJobAsync(string jobId)
    {
        if (_paused || !_jobs.TryGetValue(jobId, out var scheduler))
        {
            return ScheduleResult.NotFound;
        }

        await scheduler.RunAsync(CancellationToken.None);
        return ScheduleResult.Succeed;
    }

    public void CancelSleep()
    {
    }

    public void PersistAll()
    {
    }
}