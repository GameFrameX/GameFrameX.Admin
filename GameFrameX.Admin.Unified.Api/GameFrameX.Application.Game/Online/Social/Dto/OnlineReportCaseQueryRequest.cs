using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Online.Social.Dto;

/// <summary>
/// Online 举报案件查询请求体（Admin → Online 线缆契约；mock 先行，联调以 vault 详表回写）。
/// </summary>
public class OnlineReportCaseQueryRequest
{
    /// <summary>
    /// 案件状态过滤（null = 不过滤）。
    /// </summary>
    public OnlineReportCaseStatusKind? Status { get; set; }

    /// <summary>
    /// 举报人玩家 Id 过滤（null = 不过滤）。
    /// </summary>
    public string? ReporterPlayerId { get; set; }

    /// <summary>
    /// 举报时间窗起点（Unix 秒；null = 不限）。
    /// </summary>
    public long? StartTime { get; set; }

    /// <summary>
    /// 举报时间窗终点（Unix 秒；null = 不限）。
    /// </summary>
    public long? EndTime { get; set; }

    /// <summary>
    /// 游标（首页不传）。
    /// </summary>
    public string? Cursor { get; set; }

    /// <summary>
    /// 页大小（1～100）。
    /// </summary>
    public int PageSize { get; set; } = 20;
}
