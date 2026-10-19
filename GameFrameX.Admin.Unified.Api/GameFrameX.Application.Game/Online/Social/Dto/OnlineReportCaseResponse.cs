using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Online.Social.Dto;

/// <summary>
/// Online 举报案件单行数据（Admin → Online 线缆契约；证据链字段对齐 vault:C7 四频道举报模型；mock 先行，联调以 vault 详表回写）。
/// </summary>
public class OnlineReportCaseResponse
{
    /// <summary>
    /// 案件 Id（Online 侧标识）。
    /// </summary>
    public string CaseId { get; set; } = null!;

    /// <summary>
    /// 应用 Id。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 区服 Id。
    /// </summary>
    public long ServerId { get; set; }

    /// <summary>
    /// 举报人玩家 Id。
    /// </summary>
    public string ReporterPlayerId { get; set; } = null!;

    /// <summary>
    /// 被举报玩家 Id。
    /// </summary>
    public string ReportedPlayerId { get; set; } = null!;

    /// <summary>
    /// 举报场景（文字描述，如 chat / match）。
    /// </summary>
    public string Scene { get; set; } = null!;

    /// <summary>
    /// 关联对局 Id（对局场景举报时携带）。
    /// </summary>
    public string? MatchId { get; set; }

    /// <summary>
    /// 关联聊天消息 Id（聊天场景举报时携带）。
    /// </summary>
    public string? ChatMessageId { get; set; }

    /// <summary>
    /// 举报原因分类。
    /// </summary>
    public string Reason { get; set; } = null!;

    /// <summary>
    /// 证据内容（截图引用 / 消息快照等；结构由 Online 侧定义，Admin 只透传展示）。
    /// </summary>
    public string? Evidence { get; set; }

    /// <summary>
    /// 案件状态。
    /// </summary>
    public OnlineReportCaseStatusKind Status { get; set; }

    /// <summary>
    /// 处置结果摘要（已处置时由 Online 回写）。
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
