// GameFrameX 组织下的以及组织衍生的项目的版权、商标和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。

using System.ComponentModel.DataAnnotations;
using GameFrameX.Entity.Game.Online;

namespace GameFrameX.Application.Game.Online.LiveOps.ConfigRollout.Dto;

/// <summary>
/// 灰度回滚输入（来源 change C343 / vault:C8 S7.8 / VC-7.12）。回滚 = 版本化新下发：恢复目标版本的 SegmentId/Percent 语义，生成新 ConfigVersion 行。
/// </summary>
public sealed class RollbackConfigRolloutRequest
{
    /// <summary>
    /// 应用Id（事实数据应用作用域）。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "应用Id必须大于0")]
    public long AppId { get; set; }

    /// <summary>
    /// 目标区服Id（Online 作用域 ServerId）。
    /// </summary>
    [Required(ErrorMessage = "区服Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "区服Id必须大于0")]
    public long ServerId { get; set; }

    /// <summary>
    /// 配置族。
    /// </summary>
    [Required(ErrorMessage = "配置族不能为空")]
    public OnlineConfigRolloutKind ConfigKind { get; set; }

    /// <summary>
    /// 配置业务键（所辖配置域实体 Id 的字符串形式）。
    /// </summary>
    [Required(ErrorMessage = "配置业务键不能为空")]
    public string ConfigKey { get; set; } = null!;

    /// <summary>
    /// 回滚目标版本（要恢复到的历史 ConfigVersion，必须已存在于该配置的版本序列）。
    /// </summary>
    [Required(ErrorMessage = "回滚目标版本不能为空")]
    [Range(1, int.MaxValue, ErrorMessage = "回滚目标版本必须大于0")]
    public int TargetVersion { get; set; }

    /// <summary>
    /// 回滚理由（必填，进审计）。
    /// </summary>
    [Required(ErrorMessage = "回滚理由不能为空")]
    public string Reason { get; set; } = null!;
}
