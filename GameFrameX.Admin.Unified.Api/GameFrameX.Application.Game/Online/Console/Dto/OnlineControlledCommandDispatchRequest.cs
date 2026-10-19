namespace GameFrameX.Application.Game.Online.Console.Dto;

/// <summary>
/// 受控操作命令线缆请求（vault:C9 S8.1 契约；mock 先行，联调以 vault 详表为准）。
/// <para>作用域三元组由 OnlineScopeContext 注入，不在线缆体内重复。</para>
/// </summary>
public class OnlineControlledCommandDispatchRequest
{
    /// <summary>
    /// 受控命令标识（Online 侧命令名）。
    /// </summary>
    public string Command { get; set; } = null!;

    /// <summary>
    /// 目标 Ticket Id（CancelTicket）。
    /// </summary>
    public string? TicketId { get; set; }

    /// <summary>
    /// 目标 Match Id（EndAbnormalMatch）。
    /// </summary>
    public string? MatchId { get; set; }

    /// <summary>
    /// 目标玩家 Id（SuspendResumePlayer / MatchIsolation）。
    /// </summary>
    public string? PlayerId { get; set; }

    /// <summary>
    /// 开关方向（Apply / Lift 原值；单向命令为 null）。
    /// </summary>
    public string? Direction { get; set; }

    /// <summary>
    /// 操作原因（进 Online 侧审计）。
    /// </summary>
    public string Reason { get; set; } = null!;

    /// <summary>
    /// 复核人操作者 Id（进 Online 侧审计）。
    /// </summary>
    public string ReviewerOperatorId { get; set; } = null!;

    /// <summary>
    /// 幂等键（Online IdempotencyKey，VC-8.4：同键重复执行返回首次结果）。
    /// </summary>
    public string IdempotencyKey { get; set; } = null!;
}
