namespace GameFrameX.Application.ServerCenter.GameAreaMerge.Dto;

/// <summary>合服流程数据。</summary>
public class ApproveGameAreaMergePlanRequest
{
    public long Id { get; set; }
    [Required] public string Reason { get; set; } = string.Empty;
    public long ReviewerOperatorId { get; set; }
}
