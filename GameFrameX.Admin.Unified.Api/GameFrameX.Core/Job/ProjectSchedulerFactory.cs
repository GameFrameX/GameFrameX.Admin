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
        if (string.IsNullOrWhiteSpace(builder.JobId))
        {
            return;
        }

        _jobs[builder.JobId] = new ProjectScheduler(builder, _serviceProvider, _persistences);
    }

    public ProjectScheduler? GetJob(string jobId)
    {
        return _jobs.GetValueOrDefault(jobId);
    }

    public void RemoveJob(string jobId)
    {
        _jobs.TryRemove(jobId, out _);
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