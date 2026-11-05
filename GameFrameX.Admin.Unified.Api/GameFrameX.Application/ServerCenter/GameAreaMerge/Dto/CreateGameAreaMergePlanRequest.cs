namespace GameFrameX.Application.ServerCenter.GameAreaMerge.Dto;

/// <summary>合服流程数据。</summary>
public class CreateGameAreaMergePlanRequest
{
    public long TargetServerId { get; set; }
    public List<long> SourceServerIds { get; set; } = new();
    [Required] public string Reason { get; set; } = string.Empty;
}
