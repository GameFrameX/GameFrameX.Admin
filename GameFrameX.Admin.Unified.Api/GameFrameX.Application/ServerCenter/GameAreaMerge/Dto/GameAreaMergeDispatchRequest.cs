namespace GameFrameX.Application.ServerCenter.GameAreaMerge.Dto;

/// <summary>合服流程数据。</summary>
public class GameAreaMergeDispatchRequest
{
    public long TargetServerId { get; set; }
    public string SourceServerIds { get; set; } = string.Empty;
}
