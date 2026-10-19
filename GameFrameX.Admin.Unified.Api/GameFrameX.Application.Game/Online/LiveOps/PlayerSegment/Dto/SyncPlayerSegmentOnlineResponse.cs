// GameFrameX 组织下的以及组织衍生的项目的版权、商标和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。

using GameFrameX.Entity.Game.Online;

namespace GameFrameX.Application.Game.Online.LiveOps.PlayerSegment.Dto;

/// <summary>
/// 分群同步 Online 输出。
/// </summary>
public sealed class SyncPlayerSegmentOnlineResponse
{
    /// <summary>
    /// 分群Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 同步状态（成功 Synced / 失败 Failed——失败时异常已抛出，本字段用于成功路径回显）。
    /// </summary>
    public OnlinePlayerSegmentSyncStatus SyncStatus { get; set; }

    /// <summary>
    /// Online 侧分群标识。
    /// </summary>
    public long OnlineSegmentId { get; set; }

    /// <summary>
    /// Online 确认版本回执。
    /// </summary>
    public string? ConfirmVersion { get; set; }

    /// <summary>
    /// 提示消息（Online 侧原文）。
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 同步时间（Unix 秒，UTC）。
    /// </summary>
    public long SyncedTime { get; set; }
}
