namespace GameFrameX.Core.EventBus;

public sealed class ProjectEventBusOptions
{
    public bool UseUtcTimestamp { get; set; }

    public bool LogEnabled { get; set; }

    public void AddExecutor<TExecutor>()
    {
    }
}