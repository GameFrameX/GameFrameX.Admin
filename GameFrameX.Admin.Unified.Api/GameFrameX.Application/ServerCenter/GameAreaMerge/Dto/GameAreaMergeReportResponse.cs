namespace GameFrameX.Application.ServerCenter.GameAreaMerge.Dto;

/// <summary>合服流程数据。</summary>
public class GameAreaMergeReportResponse
{
    public string PlanId { get; set; } = string.Empty;
    public long TargetServerId { get; set; }
    public List<long> SourceServerIds { get; set; } = new();
    public bool DryRun { get; set; }
    public bool AlreadyExecuted { get; set; }
    public int RowKeySchemaVersion { get; set; }
    public long GlobalDomainRowsTouched { get; set; }
    public List<GameAreaMergeReportStepResponse> Steps { get; set; } = new();
}
