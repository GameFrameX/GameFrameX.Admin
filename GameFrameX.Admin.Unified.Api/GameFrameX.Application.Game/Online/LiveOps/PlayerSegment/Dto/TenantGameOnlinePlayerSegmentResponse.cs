// GameFrameX 组织下的以及组织衍生的项目的版权、商标和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。

using GameFrameX.Application.Game.Online.LiveOps.PlayerSegment;
using GameFrameX.Entity.Game.Online;

namespace GameFrameX.Application.Game.Online.LiveOps.PlayerSegment.Dto;

/// <summary>
/// 玩家分群输出。
/// </summary>
public sealed class TenantGameOnlinePlayerSegmentResponse
{
    /// <summary>
    /// 分群Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用Id。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 分群名称。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 分群描述。
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 分群条件 JSON（List&lt;<see cref="OnlineSegmentCondition"/>&gt;）。
    /// </summary>
    public string? ConditionsJson { get; set; }

    /// <summary>
    /// 同步状态（NotSynced/Synced/Failed）。
    /// </summary>
    public OnlinePlayerSegmentSyncStatus SyncStatus { get; set; }

    /// <summary>
    /// 最近同步时间（Unix 秒）。
    /// </summary>
    public long LastSyncTime { get; set; }

    /// <summary>
    /// 最近同步区服Id。
    /// </summary>
    public long LastSyncServerId { get; set; }

    /// <summary>
    /// Online 侧分群标识。
    /// </summary>
    public long LastOnlineSegmentId { get; set; }

    /// <summary>
    /// 最近同步结果码。
    /// </summary>
    public int LastServerCode { get; set; }

    /// <summary>
    /// 最近同步消息。
    /// </summary>
    public string? LastServerMessage { get; set; }

    /// <summary>
    /// 创建时间（Unix 秒）。
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 更新时间（Unix 秒）。
    /// </summary>
    public long UpdateTime { get; set; }

    /// <summary>
    /// 乐观锁版本号（C228：GenericCrudService TResponse 必含，乐观锁编辑链路与默认排序依赖）。
    /// </summary>
    public long RowVersion { get; set; }
}
