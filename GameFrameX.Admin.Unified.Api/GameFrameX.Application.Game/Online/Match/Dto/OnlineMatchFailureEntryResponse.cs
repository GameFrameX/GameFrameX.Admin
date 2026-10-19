namespace GameFrameX.Application.Game.Online.Match.Dto;

/// <summary>
/// 匹配失败明细行（Online 原值线缆形态；ReasonText 为 Online 诊断原文，仅排查展示，禁止按文案判断业务语义）。
/// </summary>
public class OnlineMatchFailureEntryResponse
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
    /// 失败原因码（Online 原值：Timeout/CancelRace/RateLimited/DuplicateTicket）。
    /// </summary>
    public string ReasonCode { get; set; } = string.Empty;

    /// <summary>
    /// 失败诊断原文（仅排查展示）。
    /// </summary>
    public string ReasonText { get; set; } = string.Empty;

    /// <summary>
    /// 发生时间（Unix 秒）。
    /// </summary>
    public long OccurredAt { get; set; }
}
