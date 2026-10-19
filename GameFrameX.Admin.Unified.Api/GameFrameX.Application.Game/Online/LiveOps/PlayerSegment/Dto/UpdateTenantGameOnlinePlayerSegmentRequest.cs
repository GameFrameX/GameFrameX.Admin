// GameFrameX 组织下的以及组织衍生的项目的版权、商标和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。

using GameFrameX.Entity.Game.Online;

namespace GameFrameX.Application.Game.Online.LiveOps.PlayerSegment.Dto;

/// <summary>
/// 更新玩家分群输入（编辑定义后须重新 syncOnline 生效）。
/// </summary>
public sealed class UpdateTenantGameOnlinePlayerSegmentRequest : TenantGameOnlinePlayerSegmentBaseRequest
{
    // 同步状态机字段不在 BaseRequest：Adapt 后 enum/long 默认 0 穿透 IgnoreColumns 写库，
    // 会把同步回执清零（见 OnUpdatingAsync 回填），与 MailCampaign Update 同范式。
    /// <summary>
    /// 同步状态（服务端回填，客户端不可写）。
    /// </summary>
    public OnlinePlayerSegmentSyncStatus SyncStatus { get; set; }

    /// <summary>
    /// 最近同步时间（Unix 秒；服务端回填）。
    /// </summary>
    public long LastSyncTime { get; set; }

    /// <summary>
    /// 最近同步区服Id（服务端回填）。
    /// </summary>
    public long LastSyncServerId { get; set; }

    /// <summary>
    /// Online 侧分群标识（服务端回填）。
    /// </summary>
    public long LastOnlineSegmentId { get; set; }

    /// <summary>
    /// 最近同步结果码（服务端回填）。
    /// </summary>
    public int LastServerCode { get; set; }

    /// <summary>
    /// 最近同步消息（服务端回填）。
    /// </summary>
    public string? LastServerMessage { get; set; }
}
