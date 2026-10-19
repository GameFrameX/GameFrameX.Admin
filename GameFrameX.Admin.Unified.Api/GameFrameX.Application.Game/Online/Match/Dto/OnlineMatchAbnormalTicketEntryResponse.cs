namespace GameFrameX.Application.Game.Online.Match.Dto;

/// <summary>
/// 异常 Ticket 行（Online 原值线缆形态；vault Ticket 字段裁剪——观测页无消费场景的 PlayerIds/SkillRange/TeamSize 等不进列表）。
/// </summary>
public class OnlineMatchAbnormalTicketEntryResponse
{
    /// <summary>
    /// Ticket Id。
    /// </summary>
    public string TicketId { get; set; } = string.Empty;

    /// <summary>
    /// 所属 Party Id（无队伍时为 null）。
    /// </summary>
    public string? PartyId { get; set; }

    /// <summary>
    /// 匹配模式。
    /// </summary>
    public string Mode { get; set; } = string.Empty;

    /// <summary>
    /// 地区。
    /// </summary>
    public string Region { get; set; } = string.Empty;

    /// <summary>
    /// Ticket 状态（Online 原值：Queued/Matching/Matched/Cancelled/Expired/Failed）。
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间（Unix 秒）。
    /// </summary>
    public long CreatedAt { get; set; }

    /// <summary>
    /// 过期时间（Unix 秒；未设置为 null）。
    /// </summary>
    public long? ExpiresAt { get; set; }

    /// <summary>
    /// 已等待秒数（LongWaiting 类别主看字段；未提供为 null）。
    /// </summary>
    public long? WaitingSeconds { get; set; }

    /// <summary>
    /// 累计取消次数（RepeatedCancel 类别主看字段；未提供为 null）。
    /// </summary>
    public long? CancelCount { get; set; }
}
