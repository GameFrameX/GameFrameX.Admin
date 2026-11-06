using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.ServerCenter.GameAreaMerge.Dto;

/// <summary>合服计划详情，包含阶段报告快照。</summary>
public class GameAreaMergePlanResponse
{
    public long Id { get; set; }
    public long? RowVersion { get; set; }
    public long CreatedTime { get; set; }
    public long TargetServerId { get; set; }
    public string SourceServerIds { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;
    public GameAreaMergePlanStatus Status { get; set; }
    public string? DryRunReportSnapshot { get; set; }
    public string? ExecutionReportSnapshot { get; set; }
    public long ApplicantOperatorId { get; set; }
    public long? ReviewerOperatorId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? BackupIdentifier { get; set; }
    public bool BackupConfirmed { get; set; }
    public long? DryRunTime { get; set; }
    public long? ApprovedTime { get; set; }
    public long? ExecutionStartedTime { get; set; }
    public long? CompletedTime { get; set; }
    public long? FailedTime { get; set; }
    public string? FailureMessage { get; set; }
    public long? CancelledByOperatorId { get; set; }
    public long? CancelledTime { get; set; }
    public string? CancellationReason { get; set; }
    public bool Cancellable { get; set; }
    public bool ExecutionReclaimable { get; set; }
}
