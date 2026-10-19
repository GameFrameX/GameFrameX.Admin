namespace GameFrameX.Application.Game.Online.Match.Dto;

/// <summary>
/// 异常 Ticket 分页响应（change C341 §3 A0 字段终版：Items/NextCursor/HasMore + ServerTime）。
/// </summary>
public class OnlineMatchAbnormalTicketPageResponse
{
    /// <summary>
    /// 当前页异常 Ticket 行（已做展示键映射）。
    /// </summary>
    public List<OnlineMatchAbnormalTicketResponse> Items { get; set; } = new();

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
