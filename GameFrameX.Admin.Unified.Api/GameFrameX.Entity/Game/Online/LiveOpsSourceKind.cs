// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯用户合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

using System.ComponentModel;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Entity.Game.Online;

/// <summary>
/// LiveOps 运营事件的五类来源（来源 change C343 / vault:C8 S7.7；硬枚举严格五值，守护测试锁定）。
/// <para>五类来源复用 Admin 既有运营域，本枚举是 <c>OnlineLiveOpsDispatchService</c> 统一下发入口的路由键：
/// RemoteConfig / DeviceGroup / ScheduledTask / Announcement 四类经 Online admin API 命令下发；
/// MailCampaign 复用既有 <c>MailCampaignServerClient</c>（Fulfillment 链路 <c>publish_mail_campaign</c>），
/// 不经 Online admin API。越枚举值下发在 Dispatch 入口即被拒绝（ParameterValidationFailed）。</para>
/// </summary>
[Description("LiveOps 运营事件来源")]
[AdminEnum]
public enum LiveOpsSourceKind
{
    /// <summary>
    /// 远程配置（<c>TenantGameAppRemoteConfigEntity</c>；Online 命令 <c>publishRemoteConfig</c>，配置内容快照幂等）。
    /// </summary>
    [Description("远程配置")] RemoteConfig = 1,

    /// <summary>
    /// 设备分组（<c>TenantDeviceGroupEntity</c>；Online 命令 <c>syncDeviceGroup</c>，全量快照同步幂等）。
    /// </summary>
    [Description("设备分组")] DeviceGroup = 2,

    /// <summary>
    /// 定时任务（<c>TenantGameTimeJobEntity</c>；Online 命令 <c>syncScheduledTask</c>，任务定义快照同步幂等）。
    /// </summary>
    [Description("定时任务")] ScheduledTask = 3,

    /// <summary>
    /// 玩家公告（<c>TenantGameAnnouncementEntity</c>；Online 命令 <c>pushAnnouncement</c>，公告 Id + 内容快照幂等）。
    /// </summary>
    [Description("玩家公告")] Announcement = 4,

    /// <summary>
    /// 运营邮件 Campaign（<c>TenantGameMailCampaignEntity</c>；复用既有 <c>publish_mail_campaign</c> 链路，
    /// 同目标已发布时合并返回首次回执，不重复下发）。
    /// </summary>
    [Description("邮件 Campaign")] MailCampaign = 5,
}
