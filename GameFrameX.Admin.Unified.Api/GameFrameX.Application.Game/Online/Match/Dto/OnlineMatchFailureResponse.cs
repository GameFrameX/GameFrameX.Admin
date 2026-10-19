namespace GameFrameX.Application.Game.Online.Match.Dto;

/// <summary>
/// 匹配失败明细行（Admin 展示形态 = Online 原值 + 展示键映射产物，经 <c>OnlineMatchTicketDisplayMap</c> 集中生成）。
/// </summary>
public class OnlineMatchFailureResponse
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
    /// 失败原因码（Online 原值）。
    /// </summary>
    public string ReasonCode { get; set; } = string.Empty;

    /// <summary>
    /// 失败原因展示键（onlineMatch.failureReason.&lt;小写原值&gt;；未知原值兜底 onlineMatch.failureReason.unknown）。
    /// </summary>
    public string ReasonDisplayKey { get; set; } = string.Empty;

    /// <summary>
    /// 失败诊断原文（仅排查展示）。
    /// </summary>
    public string ReasonText { get; set; } = string.Empty;

    /// <summary>
    /// 发生时间（Unix 秒）。
    /// </summary>
    public long OccurredAt { get; set; }
}
