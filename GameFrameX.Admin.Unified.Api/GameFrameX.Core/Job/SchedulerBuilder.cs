namespace GameFrameX.Core.Job;

public sealed class SchedulerBuilder : IEnumerable<TriggerBuilder>
{
    private readonly JobBuilder _jobBuilder;
    private readonly List<TriggerBuilder> _triggerBuilders;

    private SchedulerBuilder(JobBuilder jobBuilder, IEnumerable<TriggerBuilder> triggerBuilders)
    {
        _jobBuilder = jobBuilder;
        _triggerBuilders = triggerBuilders.ToList();
        foreach (var triggerBuilder in _triggerBuilders)
        {
            triggerBuilder.Trigger.JobId = jobBuilder.JobId;
        }
    }

    public static SchedulerBuilder Create(JobBuilder jobBuilder, params TriggerBuilder[] triggerBuilders)
    {
        return new SchedulerBuilder(jobBuilder, triggerBuilders);
    }

    public JobBuilder GetJobBuilder()
    {
        return _jobBuilder;
    }

    public TriggerBuilder? GetTriggerBuilder(string? triggerId)
    {
        return _triggerBuilders.FirstOrDefault(item => item.TriggerId == triggerId);
    }

    public IEnumerable<TriggerBuilder> GetEnumerable()
    {
        return this;
    }

    public void AddTriggerBuilder(TriggerBuilder triggerBuilder)
    {
        triggerBuilder.Trigger.JobId = _jobBuilder.JobId;
        _triggerBuilders.Add(triggerBuilder);
    }

    public SchedulerBuilder Updated()
    {
        return this;
    }

    public IEnumerator<TriggerBuilder> GetEnumerator()
    {
        return _triggerBuilders.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}