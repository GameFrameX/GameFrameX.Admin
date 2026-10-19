using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Online.Social.Dto;

/// <summary>
/// 举报案件处置响应（三步结果聚合：事实源写入 → 联动命令 → 当事人通知）。
/// </summary>
public class OnlineReportHandleResponse
{
    /// <summary>
    /// 案件 Id。
    /// </summary>
    public string CaseId { get; set; } = null!;

    /// <summary>
    /// 处置决策。
    /// </summary>
    public OnlineReportDecisionKind Decision { get; set; }

    /// <summary>
    /// 落账的处罚事实源行 Id（仅 Mute / Ban；其余决策为 null）。
    /// </summary>
    public long? PenaltyId { get; set; }

    /// <summary>
    /// 联动命令聚合结果（仅 Mute / Ban 且事实源已落账；联动失败时含失败明细，可经
    /// gamePenaltyEntity 页 syncOnlineRuntime 重试——事实源先行、联动可重试）。
    /// </summary>
    public OnlinePenaltyLinkResponse? LinkCommand { get; set; }

    /// <summary>
    /// 案件状态是否已回写 Online（false 时见 CaseUpdateFailureReason；案件主状态仍以此回写为准，
    /// Admin 不落镜像，回写失败须人工重查重处）。
    /// </summary>
    public bool CaseUpdated { get; set; }

    /// <summary>
    /// 案件状态回写失败原因（回写成功时为 null）。
    /// </summary>
    public string? CaseUpdateFailureReason { get; set; }

    /// <summary>
    /// 当事人通知是否送达（通知失败不阻断处置，原因见 NotifiedFailureReason）。
    /// </summary>
    public bool PlayerNotified { get; set; }

    /// <summary>
    /// 通知失败原因（送达时为 null）。
    /// </summary>
    public string? NotifiedFailureReason { get; set; }

    /// <summary>
    /// 处置完成时间（Unix 秒）。
    /// </summary>
    public long HandledAt { get; set; }
}
