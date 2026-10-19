namespace GameFrameX.Application.Game.Online.Match.Dto;

/// <summary>
/// 匹配失败原因分页响应（change C341 §3 A0 字段终版：Items/ReasonSummary/NextCursor/HasMore + ServerTime）。
/// </summary>
public class OnlineMatchFailurePageResponse
{
    /// <summary>
    /// 当前页失败明细行（已做展示键映射）。
    /// </summary>
    public List<OnlineMatchFailureResponse> Items { get; set; } = new();

    /// <summary>
    /// 按原因码聚合的摘要行（Online 原值透传）。
    /// </summary>
    public List<OnlineMatchFailureReasonSummaryResponse> ReasonSummary { get; set; } = new();

    /// <summary>
    /// 下一页游标（null = 没有更多数据）。
    /// </summary>
    public string? NextCursor { get; set; }

    /// <summary>
    /// 是否还有更多数据。
    /// </summary>
    public bool HasMore { get; set; }

    /// <summary>
    /// Online 侧快照时间（Unix 秒；每次拉取标注）。
    /// </summary>
    public long ServerTime { get; set; }
}
