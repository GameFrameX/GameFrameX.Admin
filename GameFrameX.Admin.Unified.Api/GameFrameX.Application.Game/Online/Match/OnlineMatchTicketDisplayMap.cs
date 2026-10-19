namespace GameFrameX.Application.Game.Online.Match;

/// <summary>
/// 匹配 Ticket 状态 / 失败原因 Online 原值 → 展示键映射（唯一落点，change C341 §3 A0；vault:C5）。
/// <para>
/// 映射以 vault C5 契约为唯一来源：Ticket 状态 6 值（Queued/Matching/Matched/Cancelled/Expired/Failed）、
/// 失败原因 4 值（Timeout/CancelRace/RateLimited/DuplicateTicket）穷举锁定，键集合变更须回 change 评审。
/// </para>
/// <para>
/// 禁止按文案判断业务语义——机器判断只走 Online 原值与展示键；ReasonText 等诊断原文仅排查展示。
/// 映射规则集中本文件，Service 层禁止散落 if/switch 复刻本映射（守护：单测穷举锁定键集合 + DisplayMap 是唯一公开入口）。
/// </para>
/// </summary>
public static class OnlineMatchTicketDisplayMap
{
    /// <summary>
    /// 未知 Ticket 状态兜底展示键。
    /// </summary>
    public const string TicketStatusUnknownDisplayKey = "onlineMatch.ticketStatus.unknown";

    /// <summary>
    /// 未知失败原因兜底展示键。
    /// </summary>
    public const string FailureReasonUnknownDisplayKey = "onlineMatch.failureReason.unknown";

    /// <summary>
    /// Ticket 状态展示键集合（严格等于 A0 锁定的 6 个 Online 原值穷举，按声明顺序；键集合变更须回 change 评审）。
    /// </summary>
    public static readonly string[] TicketStatusDisplayKeys =
    {
        "onlineMatch.ticketStatus.queued",
        "onlineMatch.ticketStatus.matching",
        "onlineMatch.ticketStatus.matched",
        "onlineMatch.ticketStatus.cancelled",
        "onlineMatch.ticketStatus.expired",
        "onlineMatch.ticketStatus.failed",
    };

    /// <summary>
    /// 失败原因展示键集合（严格等于 A0 锁定的 4 个 Online 原值穷举，按声明顺序；键集合变更须回 change 评审）。
    /// </summary>
    public static readonly string[] FailureReasonDisplayKeys =
    {
        "onlineMatch.failureReason.timeout",
        "onlineMatch.failureReason.cancelrace",
        "onlineMatch.failureReason.ratelimited",
        "onlineMatch.failureReason.duplicateticket",
    };

    /// <summary>
    /// Ticket 状态 Online 原值 → 展示键（onlineMatch.ticketStatus.&lt;小写原值&gt;；未知/null/空值兜底 unknown 键）。
    /// </summary>
    /// <param name="status">Online 侧 Ticket 状态原值。</param>
    /// <returns>展示键（前端 i18n 键）。</returns>
    public static string MapTicketStatusToDisplayKey(string? status)
    {
        return status switch
        {
            "Queued" => TicketStatusDisplayKeys[0],
            "Matching" => TicketStatusDisplayKeys[1],
            "Matched" => TicketStatusDisplayKeys[2],
            "Cancelled" => TicketStatusDisplayKeys[3],
            "Expired" => TicketStatusDisplayKeys[4],
            "Failed" => TicketStatusDisplayKeys[5],
            _ => TicketStatusUnknownDisplayKey,
        };
    }

    /// <summary>
    /// 失败原因 Online 原值 → 展示键（onlineMatch.failureReason.&lt;小写原值&gt;；未知/null/空值兜底 unknown 键）。
    /// </summary>
    /// <param name="reasonCode">Online 侧失败原因码原值。</param>
    /// <returns>展示键（前端 i18n 键）。</returns>
    public static string MapFailureReasonToDisplayKey(string? reasonCode)
    {
        return reasonCode switch
        {
            "Timeout" => FailureReasonDisplayKeys[0],
            "CancelRace" => FailureReasonDisplayKeys[1],
            "RateLimited" => FailureReasonDisplayKeys[2],
            "DuplicateTicket" => FailureReasonDisplayKeys[3],
            _ => FailureReasonUnknownDisplayKey,
        };
    }
}
