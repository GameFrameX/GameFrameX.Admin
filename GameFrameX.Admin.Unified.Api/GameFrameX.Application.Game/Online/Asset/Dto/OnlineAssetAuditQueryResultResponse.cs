namespace GameFrameX.Application.Game.Online.Asset.Dto;

/// <summary>
/// Online 资产审计查询响应载荷（Admin → Online 线缆契约；Cursor + HasMore；mock 先行，联调以 vault 详表回写）。
/// </summary>
public class OnlineAssetAuditQueryResultResponse
{
    /// <summary>
    /// 审计行。
    /// </summary>
    public List<OnlineAssetAuditEntryResponse> Items { get; set; } = new();

    /// <summary>
    /// 下一页游标（无更多页时为 null）。
    /// </summary>
    public string? NextCursor { get; set; }

    /// <summary>
    /// 是否还有更多页。
    /// </summary>
    public bool HasMore { get; set; }
}
