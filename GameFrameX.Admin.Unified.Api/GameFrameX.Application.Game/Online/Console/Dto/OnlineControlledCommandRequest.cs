namespace GameFrameX.Application.Game.Online.Console.Dto;

/// <summary>
/// 受控操作命令输入参数（高危端点；change C344 / vault:C9 S8.1）。
/// <para>
/// 高危复核口径（A0 锁定）：本端点整体高危（权限码仅同步 TenantAdmin），<c>Reason</c> 与
/// <c>ReviewerOperatorId</c> 对全部四命令必填——双人复核字段落地 vault「审批（如需）」，不做通用审批流。
/// 目标标识按命令取值：CancelTicket → TicketId；EndAbnormalMatch → MatchId；SuspendResumePlayer / MatchIsolation → PlayerId。
/// </para>
/// </summary>
public class OnlineControlledCommandRequest
{
    /// <summary>
    /// 应用作用域（必填，经 OnlineScopeContextFactory 校验租户归属）。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "应用Id必须大于0")]
    public long AppId { get; set; }

    /// <summary>
    /// 区服作用域（必填，经 OnlineScopeContextFactory 校验租户与应用归属）。
    /// </summary>
    [Required(ErrorMessage = "区服Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "区服Id必须大于0")]
    public long ServerId { get; set; }

    /// <summary>
    /// 受控命令（严格四值：CancelTicket / EndAbnormalMatch / SuspendResumePlayer / MatchIsolation）。
    /// </summary>
    [Required(ErrorMessage = "命令类型不能为空")]
    public OnlineControlledCommandKind Command { get; set; }

    /// <summary>
    /// 目标 Ticket Id（CancelTicket 必填；其余命令必须为空）。
    /// </summary>
    [MaxLength(EntityStringLength.Length256)]
    public string? TicketId { get; set; }

    /// <summary>
    /// 目标 Match Id（EndAbnormalMatch 必填；其余命令必须为空）。
    /// </summary>
    [MaxLength(EntityStringLength.Length256)]
    public string? MatchId { get; set; }

    /// <summary>
    /// 目标玩家 Id（SuspendResumePlayer / MatchIsolation 必填；其余命令必须为空）。
    /// </summary>
    [MaxLength(EntityStringLength.Length256)]
    public string? PlayerId { get; set; }

    /// <summary>
    /// 开关方向（SuspendResumePlayer / MatchIsolation 必填；CancelTicket / EndAbnormalMatch 必须为 null）。
    /// </summary>
    public ControlledCommandDirection? Direction { get; set; }

    /// <summary>
    /// 操作原因（必填，进审计）。
    /// </summary>
    [Required(ErrorMessage = "操作原因不能为空")]
    [MaxLength(EntityStringLength.Length512)]
    public string Reason { get; set; } = null!;

    /// <summary>
    /// 复核人操作者 Id（高危双人复核，必填，进审计；不得等于操作者本人由前端提示、后端只做非空校验）。
    /// </summary>
    [Required(ErrorMessage = "复核人不能为空")]
    [MaxLength(EntityStringLength.Length256)]
    public string ReviewerOperatorId { get; set; } = null!;
}
