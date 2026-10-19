using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Online.Social.Dto;

/// <summary>
/// 举报案件分页查询输入参数（案件数据实时透传 Online，Admin 不落镜像；作用域必填，与消息审计页一致）。
/// </summary>
public class PageOnlineReportCaseRequest
{
    /// <summary>
    /// 应用作用域（必填，经 OnlineScopeContextFactory 校验租户归属）。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "应用Id必须大于0")]
    public long AppId { get; set; }

    /// <summary>
    /// 区服作用域（必填，经 OnlineScopeContextFactory 校验租户与应用归属）。
    /// </summary>
    [Required(ErrorMessage = "区服Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "区服Id必须大于0")]
    public long ServerId { get; set; }

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
    /// 游标（上一页返回的 NextCursor；首页不传）。
    /// </summary>
    public string? Cursor { get; set; }

    /// <summary>
    /// 页大小（1～100，默认 20）。
    /// </summary>
    [Range(1, 100, ErrorMessage = "页大小必须在1~100之间")]
    public int PageSize { get; set; } = 20;
}
