namespace GameFrameX.Application.Game.Online.Asset.Dto;

/// <summary>
/// 资产审计分页响应（Cursor + HasMore，vault:C2 S1.2；透传 Online 游标语义）。
/// </summary>
public class OnlineAssetAuditPageResponse
{
    /// <summary>
    /// 当前页审计行。
    /// </summary>
    public List<OnlineAssetAuditEntryResponse> Items { get; set; } = new();

    /// <summary>
    /// 下一页游标（null = 没有更多数据）。
    /// </summary>
    public string? NextCursor { get; set; }

    /// <summary>
    /// 是否还有更多数据。
    /// </summary>
    public bool HasMore { get; set; }
}
