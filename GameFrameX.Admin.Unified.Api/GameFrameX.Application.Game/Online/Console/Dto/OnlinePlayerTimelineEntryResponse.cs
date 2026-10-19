namespace GameFrameX.Application.Game.Online.Console.Dto;

/// <summary>
/// 玩家时间线事件行（Online 事件信封原值透传；change C344 / vault:C9 S8.1）。
/// <para>全部字段来自 Online 事件信封——Admin 不构造事件、不推断归属、不补齐空位（缺事件即空槽，由前端呈现）。</para>
/// </summary>
public class OnlinePlayerTimelineEntryResponse
{
    /// <summary>
    /// 事件 Id（Online 信封 EventId）。
    /// </summary>
    public string EventId { get; set; } = string.Empty;

    /// <summary>
    /// 事件类型（Online 原值，如 session.login / match.ticket.created）。
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// 事件分组（Online 原值；与请求侧 OnlinePlayerTimelineEventGroup 枚举语义对齐，未知值前端兜底展示）。
    /// </summary>
    public string Group { get; set; } = string.Empty;

    /// <summary>
    /// 发生时间（Unix 秒）。
    /// </summary>
    public long OccurredAt { get; set; }

    /// <summary>
    /// 事件来源模块（Online 信封 Source）。
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// 关联 Id（跨模块串联键，vault:C9 可观测关联键约定）。
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// 载荷摘要（Online 侧裁剪后的 Payload 摘要，非完整载荷）。
    /// </summary>
    public string PayloadSummary { get; set; } = string.Empty;
}
