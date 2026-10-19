namespace GameFrameX.Application.Game.Online.LiveOps.Dispatch.Dto;

/// <summary>
/// 玩家分群同步 Online 结果（<c>OnlineLiveOpsDispatchService.SyncSegmentToOnlineAsync</c> 专用回执，来源 change C343 / vault:C8 S7.6）。
/// <para>由分群 Service 消费并回写分群行：Online 分配的分群标识 + 确认版本 + 通道命令名（审计展示）。</para>
/// </summary>
public sealed class OnlinePlayerSegmentSyncResponse
{
    /// <summary>
    /// 下发通道命令名（固定 <c>syncPlayerSegment</c>，审计展示）。
    /// </summary>
    public string OnlineAction { get; set; } = null!;

    /// <summary>
    /// Online 侧分配的分群标识（0 = Online 未返回，调用方不回写）。
    /// </summary>
    public long OnlineSegmentId { get; set; }

    /// <summary>
    /// Online 确认版本回执（空 = 未返回）。
    /// </summary>
    public string? ConfirmVersion { get; set; }

    /// <summary>
    /// 提示消息（Online 侧原文，回写排查辅助）。
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 同步时间（Unix 秒，UTC）。
    /// </summary>
    public long SyncedTime { get; set; }
}
