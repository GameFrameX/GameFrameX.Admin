namespace GameFrameX.Application.Game.Online.Console.Dto;

/// <summary>
/// 在线总览快照响应（Admin 出参；change C344 / vault:C9 S8.1）。
/// </summary>
public class OnlineOverviewResponse
{
    /// <summary>
    /// 在线玩家数（玩家侧连接口径，X6：不含管理员连接）。
    /// </summary>
    public long OnlinePlayerCount { get; set; }

    /// <summary>
    /// 活跃 Session 数。
    /// </summary>
    public long SessionCount { get; set; }

    /// <summary>
    /// 进行中对局数。
    /// </summary>
    public long MatchCount { get; set; }

    /// <summary>
    /// 重连率（最近时间窗重连占比，0～1）。
    /// </summary>
    public double ReconnectRate { get; set; }

    /// <summary>
    /// 匹配队列概况（按 Mode/Region 维度）。
    /// </summary>
    public List<OnlineOverviewQueueSummaryResponse> QueueSummaries { get; set; } = new List<OnlineOverviewQueueSummaryResponse>();

    /// <summary>
    /// Online 侧快照时间（Unix 秒）。
    /// </summary>
    public long ServerTime { get; set; }

    /// <summary>
    /// 快照口径说明（运行态快照非权威状态，前端不得缓存）。
    /// </summary>
    public string SnapshotNote { get; set; } = string.Empty;
}
