// GameFrameX 组织下的以及组织衍生的项目的版权、商标和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。

using System.ComponentModel.DataAnnotations;
using GameFrameX.Entity.Game.Online;

namespace GameFrameX.Application.Game.Online.LiveOps.ConfigRollout.Dto;

/// <summary>
/// 配置灰度版本行分页查询输入（来源 change C343 / vault:C8 S7.7）。
/// </summary>
public sealed class PageTenantGameOnlineConfigRolloutRequest : BasePageRequest
{
    /// <summary>
    /// 应用Id（事实数据必填，按当前应用筛选）。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }

    /// <summary>
    /// 配置族过滤。
    /// </summary>
    public OnlineConfigRolloutKind? ConfigKind { get; set; }

    /// <summary>
    /// 配置业务键精确过滤（所辖配置域实体 Id 字符串）。
    /// </summary>
    public string ConfigKey { get; set; } = null!;

    /// <summary>
    /// 发布状态过滤（Published=生效中 / RolledBack=已被替代的历史版本）。
    /// </summary>
    public OnlineConfigRolloutStatus? Status { get; set; }

    /// <summary>
    /// 目标区服Id过滤。
    /// </summary>
    public long? ServerId { get; set; }
}
