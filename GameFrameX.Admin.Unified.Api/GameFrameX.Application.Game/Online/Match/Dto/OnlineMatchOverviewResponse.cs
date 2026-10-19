namespace GameFrameX.Application.Game.Online.Match.Dto;

/// <summary>
/// 匹配队列总览响应（change C341 §3 A0 字段终版）。
/// </summary>
public class OnlineMatchOverviewResponse
{
    /// <summary>
    /// 按 Mode/Region 维度的队列快照行。
    /// </summary>
    public List<OnlineMatchQueueSnapshotResponse> Queues { get; set; } = new();

    /// <summary>
    /// Online 侧快照时间（Unix 秒；每次拉取标注，前端展示用）。
    /// </summary>
    public long ServerTime { get; set; }

    /// <summary>
    /// 快照说明（Admin 侧固定文案：运行态快照，非权威状态）。
    /// </summary>
    public string SnapshotNote { get; set; } = string.Empty;
}
