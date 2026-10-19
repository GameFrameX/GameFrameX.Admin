namespace GameFrameX.Core.Job;

public sealed class ProjectScheduler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly List<IJobPersistence> _persistences;
    private JobBuilder _jobBuilder;
    private bool _paused;

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
        return NotifyTriggerChangedAsync(builder.Trigger, PersistenceBehavior.Appended);
    }

    public Task UpdateTriggerAsync(TriggerBuilder builder)
    {
        builder.Trigger.JobId = JobId;
        return NotifyTriggerChangedAsync(builder.Trigger, PersistenceBehavior.Updated);
    }

    public Task RemoveTriggerAsync(string triggerId)
    {
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
    }

    public void StartTrigger(string triggerId)
    {
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