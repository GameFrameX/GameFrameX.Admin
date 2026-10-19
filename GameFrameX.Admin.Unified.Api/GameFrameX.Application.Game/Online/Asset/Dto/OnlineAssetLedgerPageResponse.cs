namespace GameFrameX.Application.Game.Online.Asset.Dto;

/// <summary>
/// 资产流水分页响应（Cursor + HasMore，vault:C2 S1.2；透传 Online 游标语义，不套页码偏移）。
/// </summary>
public class OnlineAssetLedgerPageResponse
{
    /// <summary>
    /// 当前页流水行。
    /// </summary>
    public List<OnlineAssetLedgerEntryResponse> Items { get; set; } = new();

    /// <summary>
    /// 下一页游标（null = 没有更多数据）。
    /// </summary>
    public string? NextCursor { get; set; }

    /// <summary>
    /// 是否还有更多数据。
    /// </summary>
    public bool HasMore { get; set; }
}
