namespace GameFrameX.Application.Game.Online.Console.Dto;

/// <summary>
/// 玩家时间线查询输入参数（change C344 / vault:C9 S8.1；Cursor 翻页，vault:C2 S1.2）。
/// </summary>
public class OnlinePlayerTimelineRequest
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
    /// 目标玩家 Id（必填）。
    /// </summary>
    [Required(ErrorMessage = "玩家Id不能为空")]
    [MaxLength(EntityStringLength.Length256)]
    public string PlayerId { get; set; } = null!;

    /// <summary>
    /// 事件分组过滤（null = 全部分组）。
    /// </summary>
    public OnlinePlayerTimelineEventGroup? EventGroup { get; set; }

    /// <summary>
    /// 时间窗起点（Unix 秒；null = 不限）。
    /// </summary>
    public long? StartTime { get; set; }

    /// <summary>
    /// 时间窗终点（Unix 秒；null = 不限）。
    /// </summary>
    public long? EndTime { get; set; }

    /// <summary>
    /// 游标（上一页返回的 NextCursor；首页不传）。
    /// </summary>
    [MaxLength(EntityStringLength.Length256)]
    public string? Cursor { get; set; }

    /// <summary>
    /// 页大小（1～100，默认 20）。
    /// </summary>
    [Range(1, 100, ErrorMessage = "页大小必须在1~100之间")]
    public int PageSize { get; set; } = 20;
}
