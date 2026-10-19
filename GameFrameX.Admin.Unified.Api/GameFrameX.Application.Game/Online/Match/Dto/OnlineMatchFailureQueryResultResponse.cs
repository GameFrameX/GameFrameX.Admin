namespace GameFrameX.Application.Game.Online.Match.Dto;

/// <summary>
/// 匹配失败原因 Online 查询响应载荷（Admin → Online 线缆契约，query_match_ticket_failures；mock 先行，联调以 vault C5 详表回写）。
/// <para>ServerTime 为 Online 侧快照时间（A0 决策：mock-first 冗余进 Data 载荷，不动 C339 信封层基座）。</para>
/// </summary>
public class OnlineMatchFailureQueryResultResponse
{
    /// <summary>
    /// Online 侧快照时间（Unix 秒）。
    /// </summary>
    public long ServerTime { get; set; }

    /// <summary>
    /// 失败明细行（Online 原值，未做展示映射）。
    /// </summary>
    public List<OnlineMatchFailureEntryResponse> Items { get; set; } = new();

    /// <summary>
    /// 按原因码聚合的摘要行。
    /// </summary>
    public List<OnlineMatchFailureReasonSummaryResponse> ReasonSummary { get; set; } = new();

    /// <summary>
    /// 下一页游标（无更多页时为 null）。
    /// </summary>
    public string? NextCursor { get; set; }

    /// <summary>
    /// 是否还有更多页。
    /// </summary>
    public bool HasMore { get; set; }
}
