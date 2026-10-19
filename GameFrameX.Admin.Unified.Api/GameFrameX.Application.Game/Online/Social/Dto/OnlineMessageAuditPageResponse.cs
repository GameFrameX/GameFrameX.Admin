namespace GameFrameX.Application.Game.Online.Social.Dto;

/// <summary>
/// 消息审计分页响应（Cursor + HasMore，vault:C2 S1.2；透传 Online 游标语义）。
/// </summary>
public class OnlineMessageAuditPageResponse
{
    /// <summary>
    /// 当前页消息条目（脱敏后）。
    /// </summary>
    public List<OnlineMessageAuditItemResponse> Items { get; set; } = new();

    /// <summary>
    /// 下一页游标（null = 没有更多数据）。
    /// </summary>
    public string? NextCursor { get; set; }

    /// <summary>
    /// 是否还有更多数据。
    /// </summary>
    public bool HasMore { get; set; }
}
