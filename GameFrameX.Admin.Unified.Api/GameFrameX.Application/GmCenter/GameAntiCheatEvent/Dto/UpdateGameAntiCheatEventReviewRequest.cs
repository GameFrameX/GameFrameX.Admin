using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.GmCenter.GameAntiCheatEvent.Dto;

/// <summary>
/// 反作弊事件人工复核输入（ReviewAsync 动作）。仅允许在 Pending 状态下迁移到 Approved/Rejected/Escalated。
/// </summary>
public class UpdateGameAntiCheatEventReviewRequest : BaseUpdateRequest
{
    /// <summary>
    /// 目标复核状态。仅允许 Approved / Rejected / Escalated（不允许回到 Pending）。
    /// </summary>
    [Required(ErrorMessage = "目标复核状态不能为空")]
    public ManualReviewStateEnum ManualReviewState { get; set; }

    /// <summary>
    /// 复核备注。可空。
    /// </summary>
    public string? ReviewNote { get; set; }
}