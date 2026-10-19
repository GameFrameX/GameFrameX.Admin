using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Online.Social.Dto;

/// <summary>
/// 举报案件条目（vault:C7 举报证据链全字段透传：举报人/被举报人/场景/MatchId/ChatMessageId/原因/证据/状态/处理结果）。
/// </summary>
public class OnlineReportCaseItemResponse
{
    /// <summary>
    /// 案件 Id（Online 侧标识，字符串形态）。
    /// </summary>
    public string CaseId { get; set; } = null!;

    /// <summary>
    /// 应用作用域。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 区服作用域。
    /// </summary>
    public long ServerId { get; set; }

    /// <summary>
    /// 举报人玩家 Id。
    /// </summary>
    public string ReporterPlayerId { get; set; } = null!;

    /// <summary>
    /// 被举报人玩家 Id。
    /// </summary>
    public string ReportedPlayerId { get; set; } = null!;

    /// <summary>
    /// 举报场景。
    /// </summary>
    public string Scene { get; set; } = null!;

    /// <summary>
    /// 关联对局 Id（证据链字段；null = 非对局内举报）。
    /// </summary>
    public string? MatchId { get; set; }

    /// <summary>
    /// 关联聊天消息 Id（证据链字段；null = 非聊天举报）。
    /// </summary>
    public string? ChatMessageId { get; set; }

    /// <summary>
    /// 举报原因。
    /// </summary>
    public string Reason { get; set; } = null!;

    /// <summary>
    /// 证据（Online 侧证据载荷引用；null = 无证据）。
    /// </summary>
    public string? Evidence { get; set; }

    /// <summary>
    /// 案件状态（Pending / Handling / Resolved / Rejected）。
    /// </summary>
    public OnlineReportCaseStatusKind Status { get; set; }

    /// <summary>
    /// 处理结果摘要（已处置案件的非空；含联动中间态描述）。
    /// </summary>
    public string? HandleResult { get; set; }

    /// <summary>
    /// 举报时间（Unix 秒）。
    /// </summary>
    public long CreatedAt { get; set; }

    /// <summary>
    /// 最近更新时间（Unix 秒）。
    /// </summary>
    public long UpdatedAt { get; set; }
}
