// GameFrameX 组织下的以及组织衍生的项目的版权、商标和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。

using GameFrameX.Entity.Game.Online;

namespace GameFrameX.Application.Game.Online.LiveOps.ConfigRollout.Dto;

/// <summary>
/// 配置灰度版本行输出（Publish / Rollback 动作回显的公共基类）。
/// </summary>
public class TenantGameOnlineConfigRolloutResponse
{
    /// <summary>
    /// 版本行Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用Id。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 目标区服Id。
    /// </summary>
    public long ServerId { get; set; }

    /// <summary>
    /// 配置族（RemoteConfig/Announcement/DeviceGroup/ScheduledTask）。
    /// </summary>
    public OnlineConfigRolloutKind ConfigKind { get; set; }

    /// <summary>
    /// 配置业务键（所辖配置域实体 Id 字符串）。
    /// </summary>
    public string ConfigKey { get; set; } = null!;

    /// <summary>
    /// Admin 侧分配的下发版本号（同配置维度递增；发布与回滚共用序列）。
    /// </summary>
    public int ConfigVersion { get; set; }

    /// <summary>
    /// 灰度分群Id。
    /// </summary>
    public long SegmentId { get; set; }

    /// <summary>
    /// 灰度百分比（1-100，语义见 OnlineRolloutSemanticMap）。
    /// </summary>
    public int Percent { get; set; }

    /// <summary>
    /// 版本行状态（Published=生效中 / RolledBack=已被替代）。
    /// </summary>
    public OnlineConfigRolloutStatus Status { get; set; }

    /// <summary>
    /// 回滚目标版本（仅回滚行有值，指向要恢复到的历史 ConfigVersion）。
    /// </summary>
    public int TargetVersion { get; set; }

    /// <summary>
    /// 本次下发理由。
    /// </summary>
    public string Reason { get; set; } = null!;

    /// <summary>
    /// Online 确认版本回执（空 = 未取得回执）。
    /// </summary>
    public string? OnlineConfirmVersion { get; set; }

    /// <summary>
    /// 下发操作人。
    /// </summary>
    public string DispatchOperator { get; set; } = null!;

    /// <summary>
    /// 下发时间（Unix 秒）。
    /// </summary>
    public long PublishedTime { get; set; }

    /// <summary>
    /// 创建时间（Unix 秒）。
    /// </summary>
    public long CreatedTime { get; set; }
}
