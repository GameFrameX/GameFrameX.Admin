using GameFrameX.Application.Game.Online.LiveOps.Dispatch.Dto;
using GameFrameX.Entity.Game.Online;
using GameFrameX.Entity.Tenant.Game.Online;

namespace GameFrameX.Application.Game.Online.LiveOps.Dispatch;

/// <summary>
/// LiveOps 统一下发单一入口契约（来源 change C343 / vault:C8 S7.7）。
/// <para>
/// 单一入口铁律：Application.Game 业务层仅本实现注入 <c>IOnlineServerClient</c>（除 C339 既有管理通道外）；
/// 分群同步 / 灰度发布与回滚 / 既有运营域页面的「经 Online 下发」动作一律经本接口转调，
/// 禁止旁路自建 HTTP 直连 Online Runtime。接口存在是为了单测 fake 注入（同 <c>IMailCampaignServerClient</c> 范式）。
/// </para>
/// <para>
/// 五类来源路由（Online admin API 命令名，冻结于 change.md A0 契约对齐块）：
/// RemoteConfig→publishRemoteConfig / DeviceGroup→syncDeviceGroup / ScheduledTask→syncScheduledTask /
/// Announcement→pushAnnouncement / MailCampaign→既有 publish 链路（publish_mail_campaign，方案C 合并语义）；
/// 分群同步与灰度发布/回滚为内部通道（syncPlayerSegment / publishConfigRollout / rollbackConfigRollout）。
/// </para>
/// </summary>
public interface IOnlineLiveOpsDispatchService
{
    /// <summary>
    /// 统一下发运营事件（五类来源单一入口；来源存在性 / 归属校验 / 越枚举拒绝在入口完成）。
    /// </summary>
    /// <param name="input">下发输入（来源 + 业务主键 + 作用域 AppId/ServerId + 理由）。</param>
    /// <returns>下发回执（通道 / 确认版本 / 是否同目标合并）。</returns>
    /// <exception cref="BusinessException">来源越枚举 / 记录不存在 / 归属不符 / 状态不允许 / Online 协议码非 0（镜像段透出）。</exception>
    Task<OnlineLiveOpsDispatchResponse> DispatchAsync(OnlineLiveOpsDispatchRequest input);

    /// <summary>
    /// 来源存在性 + 应用归属校验（不下发；灰度发布前校验 ConfigKey 指向的来源记录用）。
    /// <para>四类配置来源查各自实体并校验归属；MailCampaign 拒绝（邮件走既有链路，不参与灰度编排，见 OnlineConfigRolloutKind）。</para>
    /// </summary>
    /// <param name="source">来源类型（四类配置来源）。</param>
    /// <param name="businessKey">来源域业务主键。</param>
    /// <param name="appId">目标应用Id。</param>
    /// <exception cref="BusinessException">来源越枚举 / 记录不存在 / 归属不符 / 邮件来源。</exception>
    Task EnsureSourceExistsAsync(LiveOpsSourceKind source, long businessKey, long appId);

    /// <summary>
    /// 分群定义同步 Online（syncPlayerSegment；Online 侧分配分群标识，幂等重同步返回同一标识）。
    /// </summary>
    /// <param name="segment">已加载的分群实体（AppId 作用域事实行）。</param>
    /// <param name="serverId">目标区服Id（Online 作用域 ServerId）。</param>
    /// <param name="reason">同步理由（审计）。</param>
    /// <returns>同步结果（Online 分群标识 + 确认版本）。</returns>
    /// <exception cref="BusinessException">作用域越界 / Online 协议码非 0（镜像段透出）。</exception>
    Task<OnlinePlayerSegmentSyncResponse> SyncSegmentToOnlineAsync(TenantGameOnlinePlayerSegmentEntity segment, long serverId, string reason);

    /// <summary>
    /// 灰度发布下发 Online（publishConfigRollout；调用前版本行已由灰度 Service 分配 ConfigVersion）。
    /// </summary>
    /// <param name="rollout">已落库的发布版本行（含 AppId/ServerId/ConfigKind/ConfigKey/ConfigVersion/SegmentId/Percent）。</param>
    /// <returns>下发结果（Online 确认版本回执，原样回写版本行）。</returns>
    /// <exception cref="BusinessException">作用域越界 / Online 协议码非 0（镜像段透出）。</exception>
    Task<OnlineConfigRolloutDispatchResponse> PublishRolloutToOnlineAsync(TenantGameOnlineConfigRolloutEntity rollout);

    /// <summary>
    /// 灰度回滚下发 Online（rollbackConfigRollout；回滚 = 版本化新下发，TargetVersion 指向目标历史版本）。
    /// </summary>
    /// <param name="rollout">已落库的回滚版本行（Status=Published、TargetVersion&gt;0 的版本化新行）。</param>
    /// <returns>下发结果（Online 确认版本回执，原样回写版本行）。</returns>
    /// <exception cref="BusinessException">作用域越界 / Online 协议码非 0（镜像段透出）。</exception>
    Task<OnlineConfigRolloutDispatchResponse> RollbackRolloutToOnlineAsync(TenantGameOnlineConfigRolloutEntity rollout);
}
