namespace GameFrameX.Application.Game.Online.Console.Dto;

/// <summary>
/// 统一审计检索分页响应（Admin 出参；change C344 / vault:C9 S8.2）。
/// </summary>
public class PageOnlineAuditSearchResponse
{
    /// <summary>
    /// 统一审计行（三来源归并，按 OccurredAt 倒序）。
    /// </summary>
    public List<OnlineAuditSearchEntryResponse> Items { get; set; } = new List<OnlineAuditSearchEntryResponse>();

    /// <summary>
    /// 下一页游标（= 末行 OccurredAt Unix 秒的时间倒序 keyset；null = 没有更多）。
    /// </summary>
    public string? NextCursor { get; set; }

    /// <summary>
    /// 是否还有更多（任一来源仍有剩余即为 true）。
    /// </summary>
    public bool HasMore { get; set; }
}
