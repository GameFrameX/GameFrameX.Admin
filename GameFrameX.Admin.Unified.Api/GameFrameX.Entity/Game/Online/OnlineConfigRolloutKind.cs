// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯用户合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

using System.ComponentModel;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Entity.Game.Online;

/// <summary>
/// 灰度发布（Rollout）适用的配置族（来源 change C343 / vault:C8 S7.8）。
/// <para>与 <see cref="LiveOpsSourceKind"/> 对应关系：灰度面向「可版本化下发的配置类来源」，
/// 邮件 Campaign 走既有发布链路（自带 Campaign 版本）不参与 Rollout 灰度编排。</para>
/// </summary>
[Description("灰度发布配置族")]
[AdminEnum]
public enum OnlineConfigRolloutKind
{
    /// <summary>
    /// 远程配置（ConfigKey = <c>TenantGameAppRemoteConfigEntity.Id</c>）。
    /// </summary>
    [Description("远程配置")] RemoteConfig = 1,

    /// <summary>
    /// 玩家公告（ConfigKey = <c>TenantGameAnnouncementEntity.Id</c>）。
    /// </summary>
    [Description("玩家公告")] Announcement = 2,

    /// <summary>
    /// 设备分组（ConfigKey = <c>TenantDeviceGroupEntity.Id</c>）。
    /// </summary>
    [Description("设备分组")] DeviceGroup = 3,

    /// <summary>
    /// 定时任务（ConfigKey = <c>TenantGameTimeJobEntity.Id</c>）。
    /// </summary>
    [Description("定时任务")] ScheduledTask = 4,
}
