// GameFrameX 组织下的以及组织衍生的项目的版权、商标和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。

using System.ComponentModel.DataAnnotations;
using GameFrameX.Application.Game.Online.RolloutSemanticMap;
using GameFrameX.Entity.Game.Online;

namespace GameFrameX.Application.Game.Online.LiveOps.ConfigRollout.Dto;

/// <summary>
/// 灰度发布输入（来源 change C343 / vault:C8 S7.7）。Percent 边界常量直接引用 <c>OnlineRolloutSemanticMap</c>（禁内联第二套数值）。
/// </summary>
public sealed class PublishConfigRolloutRequest
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
    /// 配置族（四类配置来源；邮件不参与灰度编排）。
    /// </summary>
    [Required(ErrorMessage = "配置族不能为空")]
    public OnlineConfigRolloutKind ConfigKind { get; set; }

    /// <summary>
    /// 配置业务键（所辖配置域实体 Id 的字符串形式）。
    /// </summary>
    [Required(ErrorMessage = "配置业务键不能为空")]
    public string ConfigKey { get; set; } = null!;

    /// <summary>
    /// 灰度分群Id（必须为已同步 Online 的分群）。
    /// </summary>
    [Required(ErrorMessage = "分群Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "分群Id必须大于0")]
    public long SegmentId { get; set; }

    /// <summary>
    /// 灰度百分比（1-100：0 无意义即拒绝，100 = 分群内全量；语义边界见 OnlineRolloutSemanticMap）。
    /// </summary>
    [Required(ErrorMessage = "灰度百分比不能为空")]
    [Range(OnlineRolloutSemanticMap.PublishPercentMin, OnlineRolloutSemanticMap.PublishPercentMax, ErrorMessage = "灰度百分比必须在1-100之间")]
    public int Percent { get; set; }

    /// <summary>
    /// 下发理由（必填，进审计）。
    /// </summary>
    [Required(ErrorMessage = "下发理由不能为空")]
    public string Reason { get; set; } = null!;
}
