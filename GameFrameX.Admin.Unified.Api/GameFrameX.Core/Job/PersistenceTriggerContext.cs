namespace GameFrameX.Core.Job;

public sealed class PersistenceTriggerContext
{
    public PersistenceTriggerContext(Trigger trigger, PersistenceBehavior behavior)
    {
        Trigger = trigger;
        Behavior = behavior;
    }

    public Trigger Trigger { get; }
    public PersistenceBehavior Behavior { get; }
}