// GameFrameX 组织下的以及组织衍生的项目的版权、商标和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。

using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Application.Game.Online.LiveOps.PlayerSegment.Dto;

/// <summary>
/// 分群同步 Online 输入（来源 change C343 / vault:C8 S7.6）。
/// </summary>
public sealed class SyncPlayerSegmentOnlineRequest : BaseIdRequest
{
    /// <summary>
    /// 目标区服Id（Online 作用域 ServerId，经租户归属校验）。
    /// </summary>
    [Required(ErrorMessage = "区服Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "区服Id必须大于0")]
    public long ServerId { get; set; }

    /// <summary>
    /// 同步理由（必填，审计）。
    /// </summary>
    [Required(ErrorMessage = "同步理由不能为空")]
    public string Reason { get; set; } = null!;
}
