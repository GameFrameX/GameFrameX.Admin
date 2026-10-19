using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Online.Social.Dto;

/// <summary>
/// 举报案件处置输入参数（处置流：事实源写入 → 联动命令 → 当事人通知，三步状态落审计）。
/// </summary>
public class OnlineReportHandleRequest
{
    /// <summary>
    /// 应用作用域（案件所在应用，经 OnlineScopeContextFactory 校验租户归属）。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "应用Id必须大于0")]
    public long AppId { get; set; }

    /// <summary>
    /// 区服作用域（案件所在区服，经 OnlineScopeContextFactory 校验租户与应用归属）。
    /// </summary>
    [Required(ErrorMessage = "区服Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "区服Id必须大于0")]
    public long ServerId { get; set; }

    /// <summary>
    /// 案件 Id（Online 侧标识）。
    /// </summary>
    [Required(ErrorMessage = "案件Id不能为空")]
    public string CaseId { get; set; } = null!;

    /// <summary>
    /// 被举报玩家 Id（Mute / Ban 时写入事实源 TargetId；案件数据 Admin 不落镜像，由调用方随案件行携带）。
    /// </summary>
    [Required(ErrorMessage = "被举报玩家Id不能为空")]
    [MaxLength(EntityStringLength.Length256)]
    public string ReportedPlayerId { get; set; } = null!;

    /// <summary>
    /// 处置决策（Dismiss / Warn / Mute / Ban / ForwardPenalty）。
    /// </summary>
    [Required(ErrorMessage = "处置决策不能为空")]
    public OnlineReportDecisionKind Decision { get; set; }

    /// <summary>
    /// 处置原因（必填，进入事实源 Reason 与审计）。
    /// </summary>
    [Required(ErrorMessage = "处置原因不能为空")]
    [MaxLength(EntityStringLength.Length2048)]
    public string Reason { get; set; } = null!;

    /// <summary>
    /// 处罚时长（分钟）。Decision 为 Mute / Ban 时必填且大于 0；其余决策忽略。
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "处罚时长必须大于0")]
    public int? DurationMinutes { get; set; }
}
