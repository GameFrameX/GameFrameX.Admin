namespace GameFrameX.Application.Game.Online.Console.Dto;

/// <summary>
/// Online 审计事件行（线缆契约；vault:C9 S8.2，mock 先行）。
/// </summary>
public class OnlineAuditEventEntryResponse
{
    /// <summary>
    /// 审计事件 Id。
    /// </summary>
    public string EventId { get; set; } = string.Empty;

    /// <summary>
    /// 命令类型（Online 侧原值）。
    /// </summary>
    public string CommandType { get; set; } = string.Empty;

    /// <summary>
    /// 操作者 Id。
    /// </summary>
    public string OperatorId { get; set; } = string.Empty;

    /// <summary>
    /// 目标玩家 Id。
    /// </summary>
    public string? TargetPlayerId { get; set; }

    /// <summary>
    /// 发生时间（Unix 秒）。
    /// </summary>
    public long OccurredAt { get; set; }

    /// <summary>
    /// 摘要（操作原因/结果概述）。
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// 关联 Id（vault:C9 可观测关联键约定）。
    /// </summary>
    public string? CorrelationId { get; set; }
}
