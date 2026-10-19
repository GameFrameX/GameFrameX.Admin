namespace GameFrameX.Core.Job;

public sealed class PersistenceContext
{
    public PersistenceContext(JobDetail jobDetail, PersistenceBehavior behavior)
    {
        JobDetail = jobDetail;
        Behavior = behavior;
    }

    public JobDetail JobDetail { get; }
    public PersistenceBehavior Behavior { get; }
}