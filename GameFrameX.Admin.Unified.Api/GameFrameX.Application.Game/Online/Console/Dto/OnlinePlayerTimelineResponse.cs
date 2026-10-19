namespace GameFrameX.Application.Game.Online.Console.Dto;

/// <summary>
/// 玩家时间线查询响应（Admin 出参；change C344 / vault:C9 S8.1）。
/// </summary>
public class OnlinePlayerTimelineResponse
{
    /// <summary>
    /// 目标玩家 Id（回显查询锚点）。
    /// </summary>
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>
    /// 事件行（Online 事件信封原值透传；缺事件即空槽，Admin 不做本地推断补齐）。
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
