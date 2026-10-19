namespace GameFrameX.Application.Game.Online.Console.Dto;

/// <summary>
/// 受控操作命令回执（线缆/Admin 出参共用；vault:C9 S8.1 契约，mock 先行）。
/// <para>透传 Online 首次执行结果——同 IdempotencyKey 的重复调用拿到的是首次回执（VC-8.4），不产生第二次副作用。</para>
/// </summary>
public class OnlineControlledCommandAckResponse
{
    /// <summary>
    /// 命令标识（回显）。
    /// </summary>
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// 目标标识（TicketId / MatchId / PlayerId，回显）。
    /// </summary>
    public string TargetIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// 幂等键（回执即本键；同键重发返回首次结果）。
    /// </summary>
    public string IdempotencyKey { get; set; } = string.Empty;

    /// <summary>
    /// 执行状态（Online 侧原值，如 Accepted / Executed）。
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 执行时间（Unix 秒；幂等重发时为首次执行时间）。
    /// </summary>
    public long ExecutedAt { get; set; }

    /// <summary>
    /// 执行摘要（结果概述；幂等重发时说明返回首次结果）。
    /// </summary>
    public string Summary { get; set; } = string.Empty;
}
