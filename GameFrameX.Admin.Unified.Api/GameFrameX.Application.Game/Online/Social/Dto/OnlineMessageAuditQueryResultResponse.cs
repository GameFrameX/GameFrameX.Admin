namespace GameFrameX.Application.Game.Online.Social.Dto;

/// <summary>
/// Online 消息审计查询响应载荷（Admin → Online 线缆契约；mock 先行，联调以 vault 详表回写）。
/// </summary>
public class OnlineMessageAuditQueryResultResponse
{
    /// <summary>
    /// 消息行（未脱敏原文；展示脱敏由 Admin 侧 Mapper 集中执行）。
    /// </summary>
    public List<OnlineMessageAuditMessageResponse> Items { get; set; } = new();

    /// <summary>
    /// 下一页游标（无更多页时为 null）。
    /// </summary>
    public string? NextCursor { get; set; }

    /// <summary>
    /// 是否还有更多页。
    /// </summary>
    public bool HasMore { get; set; }
}
