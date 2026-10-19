namespace GameFrameX.Core.Job;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public abstract class TriggerAttribute : Attribute
{
    public string? TriggerId { get; set; }
    public string? Description { get; set; }
    public long MaxNumberOfRuns { get; set; }
    public bool RunOnStart { get; set; }
}