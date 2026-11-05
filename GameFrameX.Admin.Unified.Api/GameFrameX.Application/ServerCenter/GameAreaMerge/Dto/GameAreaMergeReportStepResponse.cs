namespace GameFrameX.Application.ServerCenter.GameAreaMerge.Dto;

/// <summary>合服流程数据。</summary>
public class GameAreaMergeReportStepResponse
{
    public string Step { get; set; } = string.Empty;
    public long AffectedCount { get; set; }
    public Dictionary<string, long> Details { get; set; } = new();
}
