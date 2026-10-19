namespace GameFrameX.Application.Game.Announcement.GameBannerAnnouncement.Dto;

/// <summary>
/// 游戏内横幅公告实体基类 - 包含 AddInput 和 UpdateInput 的公共字段
/// </summary>
public abstract class TenantGameBannerAnnouncementEntityBaseRequest : BaseUpdateRequest
{
    /// <summary>
    /// 应用Id。运营内容应用作用域，必须 > 0。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }

    /// <summary>
    /// 区服ID
    /// </summary>
    [Required(ErrorMessage = "区服ID不能为空")]
    public long AreaId { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [Required(ErrorMessage = "内容不能为空")]
    public string Content { get; set; } = null!;

    /// <summary>
    /// 内容本地化Key。可空：空 = 直接展示 Content（服务端兜底空串，实体列 NOT NULL）。
    /// </summary>
    public string? ContentLocalizationKey { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    [Required(ErrorMessage = "优先级不能为空")]
    public int Priority { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    [Required(ErrorMessage = "开始时间不能为空")]
    public long StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [Required(ErrorMessage = "结束时间不能为空")]
    public long EndTime { get; set; }

    /// <summary>
    /// 间隔时间
    /// </summary>
    [Required(ErrorMessage = "间隔时间不能为空")]
    public int Interval { get; set; }

    /// <summary>
    /// 重复次数
    /// </summary>
    [Required(ErrorMessage = "重复次数不能为空")]
    [Range(1, int.MaxValue, ErrorMessage = "重复次数必须大于0")]
    public int RepeatCount { get; set; } = 1;

    /// <summary>
    /// 是否激活
    /// </summary>
    [Required(ErrorMessage = "是否激活不能为空")]
    public bool IsActive { get; set; }
}