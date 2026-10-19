namespace GameFrameX.Application.Game.Online.Console.Dto;

/// <summary>
/// 总览队列概况行（按 Mode/Region 维度；字段与 C341 匹配队列快照对齐，运行态数据不缓存）。
/// </summary>
public class OnlineOverviewQueueSummaryResponse
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
