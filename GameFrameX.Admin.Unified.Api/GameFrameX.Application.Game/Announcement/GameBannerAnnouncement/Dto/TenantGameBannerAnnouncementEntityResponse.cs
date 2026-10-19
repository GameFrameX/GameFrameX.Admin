namespace GameFrameX.Application.Game.Announcement.GameBannerAnnouncement.Dto;

/// <summary>
/// 游戏内横幅公告输出参数
/// </summary>
public class TenantGameBannerAnnouncementEntityResponse : IBasePageResponse<Entity.Tenant.Game.Announcement.TenantGameBannerAnnouncementEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用Id。运营内容应用作用域（TenantId + AppId），必须 &gt; 0。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 区服ID
    /// </summary>
    public long AreaId { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// 内容本地化Key
    /// </summary>
    public string ContentLocalizationKey { get; set; } = null!;

    /// <summary>
    /// 用于确定公告的显示优先级。
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// 公告开始显示的时间。
    /// </summary>
    public long StartTime { get; set; }

    /// <summary>
    /// 公告结束显示的时间。
    /// </summary>
    public long EndTime { get; set; }

    /// <summary>
    /// 公告滚动的时间间隔或频率。单位秒
    /// </summary>
    public uint Interval { get; set; }

    /// <summary>
    /// 公告重复播放次数。
    /// </summary>
    public int RepeatCount { get; set; } = 1;

    /// <summary>
    /// 指示公告当前是否应该被激活或显示。
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// 行版本号（乐观锁）
    /// </summary>
    public long? RowVersion { get; set; }
}