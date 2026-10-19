namespace GameFrameX.Application.Game.Online.Console.Dto;

/// <summary>
/// 在线总览快照线缆响应（vault:C9 契约；mock 先行，联调以 vault 详表为准）。
/// <para>全部为 Online 玩家侧运行态口径——不含管理员浏览器连接（X6），ServerTime 由 Online 侧生成。</para>
/// </summary>
public class OnlineOverviewQueryResultResponse
{
    /// <summary>
    /// 在线玩家数（玩家侧连接口径）。
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
}
