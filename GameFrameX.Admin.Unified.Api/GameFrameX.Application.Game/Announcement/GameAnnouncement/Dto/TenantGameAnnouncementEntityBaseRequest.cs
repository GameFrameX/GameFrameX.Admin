namespace GameFrameX.Application.Game.Announcement.GameAnnouncement.Dto;

/// <summary>
/// 游戏公告新增/更新共有输入参数。
/// </summary>
public class TenantGameAnnouncementEntityBaseRequest : BaseAddRequest, ITenantGameAnnouncementEntityRequest
{
    /// <summary>
    /// 应用Id。运营内容应用作用域，必须 &gt; 0。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }

    /// <inheritdoc />
    public string Language { get; set; } = null!;

    /// <inheritdoc />
    public long? AreaId { get; set; }

    /// <inheritdoc />
    [Required(ErrorMessage = "标题不能为空")]
    public string Title { get; set; } = null!;

    /// <inheritdoc />
    public string TitleLocalizationKey { get; set; } = string.Empty;

    /// <inheritdoc />
    [Required(ErrorMessage = "内容不能为空")]
    public string Content { get; set; } = null!;

    /// <inheritdoc />
    public string ContentLocalizationKey { get; set; } = string.Empty;

    /// <inheritdoc />
    [Required(ErrorMessage = "开始时间不能为空")]
    public long StartTime { get; set; }

    /// <inheritdoc />
    [Required(ErrorMessage = "结束时间不能为空")]
    public long EndTime { get; set; }

    /// <inheritdoc />
    [Required(ErrorMessage = "优先级不能为空")]
    public int Priority { get; set; }

    /// <inheritdoc />
    [Required(ErrorMessage = "是否激活不能为空")]
    public bool IsActive { get; set; }
}