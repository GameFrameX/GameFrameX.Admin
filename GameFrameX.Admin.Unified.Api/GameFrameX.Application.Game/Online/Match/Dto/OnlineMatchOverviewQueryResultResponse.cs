namespace GameFrameX.Application.Game.Online.Match.Dto;

/// <summary>
/// 队列总览 Online 查询响应载荷（Admin → Online 线缆契约，query_match_queue_overview；mock 先行，联调以 vault C5 详表回写）。
/// <para>ServerTime 为 Online 侧快照时间（A0 决策：mock-first 冗余进 Data 载荷，不动 C339 信封层基座）。</para>
/// </summary>
public class OnlineMatchOverviewQueryResultResponse
{
    /// <summary>
    /// Online 侧快照时间（Unix 秒）。
    /// </summary>
    public long ServerTime { get; set; }

    /// <summary>
    /// 按 Mode/Region 维度的队列快照行。
    /// </summary>
    public List<OnlineMatchQueueSnapshotResponse> Queues { get; set; } = new();
}
