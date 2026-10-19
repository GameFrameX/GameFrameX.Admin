namespace GameFrameX.Client.Api.GameAnnouncement.Dto;

/// <summary>
/// 公告列表请求。
/// </summary>
public class GameAnnouncementListRequest
{
    /// <summary>
    /// 区服Id。为空或小于等于 0 时只查询全服公告。
    /// </summary>
    public long? Id { get; set; }

    /// <summary>
    /// 语言。为空时使用 default。
    /// </summary>
    public string Language { get; set; } = null!;
}