using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Online.Social.Dto;

/// <summary>
/// Online 举报案件状态回写请求体（Admin → Online 线缆契约；mock 先行，联调以 vault 详表回写）。
/// </summary>
public class OnlineReportCaseUpdateRequest
{
    /// <summary>
    /// 案件 Id（Online 侧标识）。
    /// </summary>
    public string CaseId { get; set; } = null!;

    /// <summary>
    /// 处置后案件状态（Dismiss→Rejected；Warn/Mute/Ban→Resolved；ForwardPenalty→Handling）。
    /// </summary>
    public OnlineReportCaseStatusKind Status { get; set; }

    /// <summary>
    /// 处置结果摘要（决策 + 原因 + 处罚 Id 回链，Online 侧展示给举报人）。
    /// </summary>
    public string HandleResult { get; set; } = null!;
}
