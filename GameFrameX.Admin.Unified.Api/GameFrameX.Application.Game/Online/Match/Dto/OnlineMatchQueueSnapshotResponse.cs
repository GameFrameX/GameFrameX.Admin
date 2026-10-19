namespace GameFrameX.Application.Game.Online.Match.Dto;

/// <summary>
/// 匹配队列快照行（按 Mode/Region 维度；运行态快照，Admin 不缓存为权威状态）。
/// </summary>
public class OnlineMatchQueueSnapshotResponse
{
    /// <summary>
    /// 匹配模式。
    /// </summary>
    public string Mode { get; set; } = string.Empty;

    /// <summary>
    /// 地区。
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// 队列深度（当前排队 Ticket 数）。
    /// </summary>
    public long QueueDepth { get; set; }

    /// <summary>
    /// 平均等待秒数。
    /// </summary>
    public long AverageWaitSeconds { get; set; }

    /// <summary>
    /// 每分钟吞吐（最近时间窗匹配完成数）。
    /// </summary>
    public long ThroughputPerMinute { get; set; }
}
