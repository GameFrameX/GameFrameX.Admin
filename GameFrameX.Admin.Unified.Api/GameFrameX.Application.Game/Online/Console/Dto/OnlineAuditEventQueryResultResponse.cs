namespace GameFrameX.Application.Game.Online.Console.Dto;

/// <summary>
/// Online 审计事件查询线缆响应（vault:C9 S8.2 契约；mock 先行）。
/// </summary>
public class OnlineAuditEventQueryResultResponse
{
    /// <summary>
    /// 审计事件行。
    /// </summary>
    public List<OnlineAuditEventEntryResponse> Items { get; set; } = new List<OnlineAuditEventEntryResponse>();

    /// <summary>
    /// 下一页游标（null = 没有更多）。
    /// </summary>
    public string? NextCursor { get; set; }

    /// <summary>
    /// 是否还有更多。
    /// </summary>
    public bool HasMore { get; set; }
}
