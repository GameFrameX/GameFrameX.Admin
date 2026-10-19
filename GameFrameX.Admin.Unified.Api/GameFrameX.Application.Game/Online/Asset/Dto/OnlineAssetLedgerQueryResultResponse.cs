namespace GameFrameX.Application.Game.Online.Asset.Dto;

/// <summary>
/// Online 资产流水查询响应载荷（Admin → Online 线缆契约；Cursor + HasMore，vault:C2 S1.2；mock 先行，联调以 vault 详表回写）。
/// </summary>
public class OnlineAssetLedgerQueryResultResponse
{
    /// <summary>
    /// 流水行。
    /// </summary>
    public List<OnlineAssetLedgerEntryResponse> Items { get; set; } = new();

    /// <summary>
    /// 下一页游标（无更多页时为 null）。
    /// </summary>
    public string? NextCursor { get; set; }

    /// <summary>
    /// 是否还有更多页。
    /// </summary>
    public bool HasMore { get; set; }
}
