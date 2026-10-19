namespace GameFrameX.Application.Game.Online.Social.Dto;

/// <summary>
/// 举报案件分页响应（Cursor + HasMore，透传 Online 游标语义）。
/// </summary>
public class OnlineReportCasePageResponse
{
    /// <summary>
    /// 当前页案件条目。
    /// </summary>
    public List<OnlineReportCaseItemResponse> Items { get; set; } = new();

    /// <summary>
    /// 下一页游标（null = 没有更多数据）。
    /// </summary>
    public string? NextCursor { get; set; }

    /// <summary>
    /// 是否还有更多数据。
    /// </summary>
    public bool HasMore { get; set; }
}
