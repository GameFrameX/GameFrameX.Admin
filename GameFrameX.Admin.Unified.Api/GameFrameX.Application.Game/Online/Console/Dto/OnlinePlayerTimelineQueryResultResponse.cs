namespace GameFrameX.Application.Game.Online.Console.Dto;

/// <summary>
/// 玩家时间线查询线缆响应（vault:C9 契约；mock 先行，联调以 vault 详表为准）。
/// </summary>
public class OnlinePlayerTimelineQueryResultResponse
{
    /// <summary>
    /// 事件行（Online 事件信封原值）。
    /// </summary>
    public List<OnlinePlayerTimelineEntryResponse> Events { get; set; } = new List<OnlinePlayerTimelineEntryResponse>();

    /// <summary>
    /// 下一页游标（null = 没有更多）。
    /// </summary>
    public string? NextCursor { get; set; }

    /// <summary>
    /// 是否还有更多事件。
    /// </summary>
    public bool HasMore { get; set; }
}
