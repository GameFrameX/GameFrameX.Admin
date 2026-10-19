namespace GameFrameX.Application.Game.Announcement.GameBannerAnnouncement.Dto;

/// <summary>
/// 游戏内横幅公告分页查询输入参数
/// </summary>
public class PageTenantGameBannerAnnouncementEntityRequest : BaseAreaPageRequest
{
    /// <summary>
    /// 应用Id
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// 优先级
    /// </summary>
    public int? Priority { get; set; }

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

    /// <summary>
    /// 间隔时间
    /// </summary>
    public int? Interval { get; set; }

    /// <summary>
    /// 重复次数
    /// </summary>
    public int? RepeatCount { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool? IsActive { get; set; }
}