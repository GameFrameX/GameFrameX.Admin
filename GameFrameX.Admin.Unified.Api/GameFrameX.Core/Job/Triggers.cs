namespace GameFrameX.Core.Job;

public static class Triggers
{
    public static TriggerBuilder Create(string? assemblyName, string? triggerType)
    {
        return TriggerBuilder.Create(Guid.NewGuid().ToString("N")).SetTriggerType(assemblyName ?? string.Empty, triggerType ?? string.Empty);
    }
}