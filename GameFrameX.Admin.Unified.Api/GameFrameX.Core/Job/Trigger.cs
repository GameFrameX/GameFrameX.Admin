using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Job;

/// <summary>
/// 触发器元数据。
/// </summary>
public sealed class Trigger
{
    public string? TriggerId { get; set; }
    public string? JobId { get; set; }
    public string? TriggerType { get; set; }
    public string? AssemblyName { get; set; }
    public string? Args { get; set; }
    public string? Description { get; set; }
    public TriggerStatus Status { get; set; } = TriggerStatus.Ready;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime? LastRunTime { get; set; }
    public DateTime? NextRunTime { get; set; }
    public long NumberOfRuns { get; set; }
    public long MaxNumberOfRuns { get; set; }
    public long NumberOfErrors { get; set; }
    public long MaxNumberOfErrors { get; set; }
    public int NumRetries { get; set; }
    public int RetryTimeout { get; set; } = 1000;
    public bool StartNow { get; set; } = true;
    public bool RunOnStart { get; set; }
    public bool ResetOnlyOnce { get; set; } = true;
    public DateTime? UpdatedTime { get; set; }
}