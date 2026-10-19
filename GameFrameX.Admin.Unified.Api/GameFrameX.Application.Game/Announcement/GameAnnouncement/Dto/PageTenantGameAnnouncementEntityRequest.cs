namespace GameFrameX.Application.Game.Announcement.GameAnnouncement.Dto;

/// <summary>
/// 游戏公告分页查询输入参数
/// </summary>
public class PageTenantGameAnnouncementEntityRequest : BaseAreaPageRequest
{
    /// <summary>
    /// 应用Id
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 语言
    /// </summary>
    public string Language { get; set; } = null!;

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 开始时间
    /// </summary>
    public long? StartTime { get; set; }

    /// <summary>
    /// 开始时间范围
    /// </summary>
    public List<long?> StartTimeRange { get; set; } = null!;

    /// <summary>
    /// 结束时间
    /// </summary>
    public long? EndTime { get; set; }

    /// <summary>
    /// 结束时间范围
    /// </summary>
    public List<long?> EndTimeRange { get; set; } = null!;
}