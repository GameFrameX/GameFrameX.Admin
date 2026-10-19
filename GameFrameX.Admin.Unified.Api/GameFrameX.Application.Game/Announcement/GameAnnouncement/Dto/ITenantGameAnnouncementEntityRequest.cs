namespace GameFrameX.Application.Game.Announcement.GameAnnouncement.Dto;

/// <summary>
/// 游戏公告输入参数公共契约。
/// </summary>
public interface ITenantGameAnnouncementEntityRequest
{
    /// <summary>
    /// 语言。default 表示兜底公告。
    /// </summary>
    string Language { get; set; }

    /// <summary>
    /// 区服ID。为空表示全服公告。
    /// </summary>
    long? AreaId { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    string Title { get; set; }

    /// <summary>
    /// 标题本地化Key
    /// </summary>
    string TitleLocalizationKey { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    string Content { get; set; }

    /// <summary>
    /// 内容本地化Key
    /// </summary>
    string ContentLocalizationKey { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    long StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    long EndTime { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    int Priority { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    bool IsActive { get; set; }
}