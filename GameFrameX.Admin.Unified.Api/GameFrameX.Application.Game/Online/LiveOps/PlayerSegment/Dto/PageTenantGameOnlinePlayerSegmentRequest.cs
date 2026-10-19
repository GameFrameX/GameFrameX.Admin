// GameFrameX 组织下的以及组织衍生的项目的版权、商标和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。

using GameFrameX.Entity.Game.Online;

namespace GameFrameX.Application.Game.Online.LiveOps.PlayerSegment.Dto;

/// <summary>
/// 玩家分群分页查询输入。
/// </summary>
public sealed class PageTenantGameOnlinePlayerSegmentRequest : BasePageRequest
{
    /// <summary>
    /// 应用Id（事实数据必填，按当前应用筛选）。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }

    /// <summary>
    /// 名称模糊过滤。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 同步状态过滤。
    /// </summary>
    public OnlinePlayerSegmentSyncStatus? SyncStatus { get; set; }
}
