// GameFrameX 组织下的以及组织衍生的项目的版权、商标和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯用户合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using GameFrameX.Application.Game.Online.LiveOps.Dispatch.Dto;
using GameFrameX.Application.Game.ServerManager.Online;
using GameFrameX.Core.Enum.Errors;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Game.Mail;
using GameFrameX.Entity.Game.Online;
using GameFrameX.Entity.Tenant.Device;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Entity.Tenant.Game.Announcement;
using GameFrameX.Entity.Tenant.Game.Online;
using GameFrameX.Entity.Tenant.Game.RemoteConfig;

namespace GameFrameX.Application.Game.Online.LiveOps.Dispatch;

/// <summary>
/// LiveOps 统一下发单一入口服务（来源 change C343 / vault:C8 S7.7）。路由前缀 /api/onlineLiveOpsDispatch（去后缀 + lowerCamelCase）。
/// <para>
/// 单一入口铁律：本服务是 Application.Game 业务层唯一注入 <see cref="IOnlineServerClient"/> 的业务类
/// （C339 既有管理通道除外），分群 / 灰度 / 既有运营域页面的「经 Online 下发」动作全部经
/// <see cref="IOnlineLiveOpsDispatchService"/> 转调，禁止旁路自建 HTTP 直连 Online Runtime——
/// 作用域注入（Factory 三元组）、协议码镜像、审计摘要只在此处发生一次，由 OnlineLiveOpsSingleEntryTests 守护。
/// </para>
/// <para>
/// 五类来源路由表（Online admin API 命令名冻结于 change.md A0 契约对齐块，调整须回 change 评审）：
/// <list type="table">
/// <listheader><term>来源</term><term>Online 命令</term><term>生效语义</term><term>幂等依据</term></listheader>
/// <item><term>RemoteConfig</term><description>publishRemoteConfig</description><description>按配置记录整体发布，Online 以最新 Content 生效</description><description>同 ConfigId 重复发布 = 覆盖式最新版，Online 天然幂等</description></item>
/// <item><term>DeviceGroup</term><description>syncDeviceGroup</description><description>设备分群定义同步，Online 重建分群成员视图</description><description>定义快照同步，重复同步收敛到同一状态</description></item>
/// <item><term>ScheduledTask</term><description>syncScheduledTask</description><description>定时任务定义同步，Online 按定义调度</description><description>定义快照同步，重复同步收敛到同一状态</description></item>
/// <item><term>Announcement</term><description>pushAnnouncement</description><description>公告推送，按公告记录当前内容生效</description><description>同 AnnouncementId 重复推送 = 覆盖式最新版</description></item>
/// <item><term>MailCampaign</term><description>publish_mail_campaign（既有链路标识）</description><description>不重复下发：仅收 Published，同目标合并返回首次发布回执（方案C）</description><description>发布动作本身由既有 Campaign publish 链路完成且幂等，本入口只做合并回执</description></item>
/// </list>
/// </para>
/// <para>入口校验序：越枚举拒绝（<see cref="Enum.IsDefined"/>）→ 来源记录存在性 → 应用归属（AppId 与来源事实行一致）→
/// 作用域三元组（Factory：TenantId 鉴权强制注入 + AppId/ServerId 租户归属）→ Online 调用（非 0 协议码由 Client 按八段镜像抛出）。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class OnlineLiveOpsDispatchService : IOnlineLiveOpsDispatchService, IAdminApiController
{
    /// <summary>Online 命令：远程配置发布（来源路由表见类注释；命令名冻结于 change.md A0）。</summary>
    public const string ActionPublishRemoteConfig = "publishRemoteConfig";

    /// <summary>Online 命令：设备分群定义同步。</summary>
    public const string ActionSyncDeviceGroup = "syncDeviceGroup";

    /// <summary>Online 命令：定时任务定义同步。</summary>
    public const string ActionSyncScheduledTask = "syncScheduledTask";

    /// <summary>Online 命令：公告推送。</summary>
    public const string ActionPushAnnouncement = "pushAnnouncement";

    /// <summary>Online 命令：玩家分群定义同步（内部通道，分群 Service 专用）。</summary>
    public const string ActionSyncPlayerSegment = "syncPlayerSegment";

    /// <summary>Online 命令：配置灰度发布（内部通道，灰度版本行 Service 专用）。</summary>
    public const string ActionPublishConfigRollout = "publishConfigRollout";

    /// <summary>Online 命令：配置灰度回滚（内部通道；回滚 = 版本化新下发，TargetVersion 指向目标历史版本）。</summary>
    public const string ActionRollbackConfigRollout = "rollbackConfigRollout";

    /// <summary>邮件来源通道标识：复用既有 Campaign publish 链路（publish_mail_campaign），不经 Online admin API。</summary>
    public const string ChannelPublishMailCampaign = "publish_mail_campaign";

    private readonly IOnlineServerClient _onlineServerClient;
    private readonly OnlineScopeContextFactory _scopeFactory;
    private readonly SqlSugarRepository<TenantGameAppRemoteConfigEntity> _remoteConfigRepository;
    private readonly SqlSugarRepository<TenantGameAppDeviceGroupEntity> _deviceGroupRepository;
    private readonly SqlSugarRepository<TenantGameTimeJobEntity> _timeJobRepository;
    private readonly SqlSugarRepository<TenantGameAnnouncementEntity> _announcementRepository;
    private readonly SqlSugarRepository<TenantGameMailCampaignEntity> _mailCampaignRepository;

    /// <summary>
    ///
    /// </summary>
    /// <param name="onlineServerClient"></param>
    /// <param name="scopeFactory"></param>
    /// <param name="remoteConfigRepository"></param>
    /// <param name="deviceGroupRepository"></param>
    /// <param name="timeJobRepository"></param>
    /// <param name="announcementRepository"></param>
    /// <param name="mailCampaignRepository"></param>
    public OnlineLiveOpsDispatchService(IOnlineServerClient onlineServerClient,
        OnlineScopeContextFactory scopeFactory,
        SqlSugarRepository<TenantGameAppRemoteConfigEntity> remoteConfigRepository,
        SqlSugarRepository<TenantGameAppDeviceGroupEntity> deviceGroupRepository,
        SqlSugarRepository<TenantGameTimeJobEntity> timeJobRepository,
        SqlSugarRepository<TenantGameAnnouncementEntity> announcementRepository,
        SqlSugarRepository<TenantGameMailCampaignEntity> mailCampaignRepository)
    {
        _onlineServerClient = onlineServerClient;
        _scopeFactory = scopeFactory;
        _remoteConfigRepository = remoteConfigRepository;
        _deviceGroupRepository = deviceGroupRepository;
        _timeJobRepository = timeJobRepository;
        _announcementRepository = announcementRepository;
        _mailCampaignRepository = mailCampaignRepository;
    }

    /// <summary>
    /// 统一下发运营事件（五类来源单一入口）。校验序与来源路由表见类注释。
    /// </summary>
    /// <param name="input">下发输入。</param>
    /// <returns>下发回执（通道 / 确认版本 / 是否同目标合并）。</returns>
    [HttpPost("dispatch")]
    [ApiDescriptionSettings(Name = "Dispatch")]
    [DisplayName("统一下发运营事件")]
    public async Task<OnlineLiveOpsDispatchResponse> DispatchAsync(OnlineLiveOpsDispatchRequest input)
    {
        if (!Enum.IsDefined(typeof(LiveOpsSourceKind), input.Source))
        {
            // 枚举值越界（如伪造 99）：ASP.NET 绑定不拒绝未知数值，入口显式拦截，杜绝落到 default 分支被静默路由。
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { source = input.Source, });
        }

        if (string.IsNullOrWhiteSpace(input.Reason))
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { reason = input.Reason, });
        }

        string channel;
        OnlineLiveOpsResponse? ack = null;
        var merged = false;
        string? confirmVersion = null;
        string? message = null;

        switch (input.Source)
        {
            case LiveOpsSourceKind.RemoteConfig:
            {
                channel = ActionPublishRemoteConfig;
                await EnsureSourceExistsAsync(LiveOpsSourceKind.RemoteConfig, input.BusinessKey, input.AppId);
                ack = await PostOnlineAsync(ActionPublishRemoteConfig, new { ConfigId = input.BusinessKey, Reason = input.Reason, }, input);
                break;
            }

            case LiveOpsSourceKind.DeviceGroup:
            {
                channel = ActionSyncDeviceGroup;
                await EnsureSourceExistsAsync(LiveOpsSourceKind.DeviceGroup, input.BusinessKey, input.AppId);
                ack = await PostOnlineAsync(ActionSyncDeviceGroup, new { DeviceGroupId = input.BusinessKey, Reason = input.Reason, }, input);
                break;
            }

            case LiveOpsSourceKind.ScheduledTask:
            {
                channel = ActionSyncScheduledTask;
                await EnsureSourceExistsAsync(LiveOpsSourceKind.ScheduledTask, input.BusinessKey, input.AppId);
                ack = await PostOnlineAsync(ActionSyncScheduledTask, new { JobId = input.BusinessKey, Reason = input.Reason, }, input);
                break;
            }

            case LiveOpsSourceKind.Announcement:
            {
                channel = ActionPushAnnouncement;
                await EnsureSourceExistsAsync(LiveOpsSourceKind.Announcement, input.BusinessKey, input.AppId);
                ack = await PostOnlineAsync(ActionPushAnnouncement, new { AnnouncementId = input.BusinessKey, Reason = input.Reason, }, input);
                break;
            }

            case LiveOpsSourceKind.MailCampaign:
            {
                // 方案C：邮件不经 Online admin API 下发。发布动作由既有 Campaign publish 链路完成（幂等且有完整状态机），
                // 本入口只收 Published：同目标（ServerId == AreaId）合并返回首次发布回执，其余状态拒绝并指引走既有链路。
                channel = ChannelPublishMailCampaign;
                var campaign = await _mailCampaignRepository.GetFirstAsync(u => u.Id == input.BusinessKey)
                               ?? throw BusinessError.Oh(CommonErrorCode.RecordNotFound, new { input.BusinessKey, });
                EnsureSourceAppScope(campaign.AppId, input.AppId, "运营邮件Campaign");

                if (campaign.PublishStatus != MailCampaignPublishStatus.Published)
                {
                    throw BusinessError.Oh(MailCampaignErrorCode.StateOperationForbidden,
                        new { status = campaign.PublishStatus.ToString(), guidance = "请先经 Campaign 发布链路（publish）发布成功后再统一下发", });
                }

                if (input.ServerId != campaign.AreaId)
                {
                    throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed,
                        new { input.ServerId, campaign.AreaId, message = "邮件Campaign 已发布到其归属区服，下发 ServerId 必须与 AreaId 一致", });
                }

                merged = true;
                confirmVersion = campaign.PublishVersion > 0 ? campaign.PublishVersion.ToString() : null;
                message = "同目标合并：返回首次发布回执（未重复下发）";
                break;
            }

            default:
                // Enum.IsDefined 前置拦截后理论不可达；保留兜底拒绝（不静默、不误判成功）。
                throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { source = input.Source, });
        }

        if (ack != null)
        {
            confirmVersion = ack.ConfirmVersion;
            message = ack.Message;
        }

        return new OnlineLiveOpsDispatchResponse
        {
            Source = input.Source,
            BusinessKey = input.BusinessKey,
            DispatchChannel = channel,
            OnlineConfirmVersion = confirmVersion,
            Merged = merged,
            Message = message,
            DispatchedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        };
    }

    /// <inheritdoc />
    [NonAction]
    public async Task<OnlinePlayerSegmentSyncResponse> SyncSegmentToOnlineAsync(TenantGameOnlinePlayerSegmentEntity segment, long serverId, string reason)
    {
        var scope = await _scopeFactory.CreateAsync(segment.AppId, serverId, ActionSyncPlayerSegment + ":" + segment.Id);
        var ack = await _onlineServerClient.PostAsync<OnlineLiveOpsResponse>(ActionSyncPlayerSegment,
            new
            {
                SegmentId = segment.LastOnlineSegmentId,
                Name = segment.Name,
                Conditions = segment.ConditionsJson ?? string.Empty,
                Reason = reason,
            },
            scope);

        return new OnlinePlayerSegmentSyncResponse
        {
            OnlineAction = ActionSyncPlayerSegment,
            OnlineSegmentId = ack.SegmentId,
            ConfirmVersion = ack.ConfirmVersion,
            Message = ack.Message,
            SyncedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        };
    }

    /// <inheritdoc />
    [NonAction]
    public async Task<OnlineConfigRolloutDispatchResponse> PublishRolloutToOnlineAsync(TenantGameOnlineConfigRolloutEntity rollout)
    {
        var scope = await _scopeFactory.CreateAsync(rollout.AppId, rollout.ServerId, ActionPublishConfigRollout + ":" + rollout.ConfigKey + ":v" + rollout.ConfigVersion);
        var ack = await _onlineServerClient.PostAsync<OnlineLiveOpsResponse>(ActionPublishConfigRollout,
            new
            {
                ConfigKind = rollout.ConfigKind,
                ConfigKey = rollout.ConfigKey,
                ConfigVersion = rollout.ConfigVersion,
                SegmentId = rollout.SegmentId,
                Percent = rollout.Percent,
                Reason = rollout.Reason,
            },
            scope);

        return ToRolloutResult(ActionPublishConfigRollout, ack);
    }

    /// <inheritdoc />
    [NonAction]
    public async Task<OnlineConfigRolloutDispatchResponse> RollbackRolloutToOnlineAsync(TenantGameOnlineConfigRolloutEntity rollout)
    {
        // 回滚 = 版本化新下发：以新 ConfigVersion 行下发 rollbackConfigRollout，TargetVersion 指向要恢复的历史版本（语义对照见 OnlineRolloutSemanticMap）。
        var scope = await _scopeFactory.CreateAsync(rollout.AppId, rollout.ServerId, ActionRollbackConfigRollout + ":" + rollout.ConfigKey + ":v" + rollout.ConfigVersion + "->" + rollout.TargetVersion);
        var ack = await _onlineServerClient.PostAsync<OnlineLiveOpsResponse>(ActionRollbackConfigRollout,
            new
            {
                ConfigKind = rollout.ConfigKind,
                ConfigKey = rollout.ConfigKey,
                ConfigVersion = rollout.ConfigVersion,
                TargetVersion = rollout.TargetVersion,
                Reason = rollout.Reason,
            },
            scope);

        return ToRolloutResult(ActionRollbackConfigRollout, ack);
    }

    /// <summary>
    /// 来源存在性 + 应用归属校验（不下发；灰度发布前校验 ConfigKey 指向的来源记录复用此处，避免散落第二套来源 switch）。
    /// </summary>
    [NonAction]
    public async Task EnsureSourceExistsAsync(LiveOpsSourceKind source, long businessKey, long appId)
    {
        if (!Enum.IsDefined(typeof(LiveOpsSourceKind), source))
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { source, });
        }

        switch (source)
        {
            case LiveOpsSourceKind.RemoteConfig:
            {
                var entity = await _remoteConfigRepository.GetFirstAsync(u => u.Id == businessKey)
                             ?? throw BusinessError.Oh(CommonErrorCode.RecordNotFound, new { businessKey, });
                EnsureSourceAppScope(entity.AppId, appId, "客户端远程配置");
                break;
            }

            case LiveOpsSourceKind.DeviceGroup:
            {
                var entity = await _deviceGroupRepository.GetFirstAsync(u => u.Id == businessKey)
                             ?? throw BusinessError.Oh(CommonErrorCode.RecordNotFound, new { businessKey, });
                EnsureSourceAppScope(entity.AppId, appId, "设备分群");
                break;
            }

            case LiveOpsSourceKind.ScheduledTask:
            {
                var entity = await _timeJobRepository.GetFirstAsync(u => u.Id == businessKey)
                             ?? throw BusinessError.Oh(CommonErrorCode.RecordNotFound, new { businessKey, });
                EnsureSourceAppScope(entity.AppId, appId, "定时任务");
                break;
            }

            case LiveOpsSourceKind.Announcement:
            {
                var entity = await _announcementRepository.GetFirstAsync(u => u.Id == businessKey)
                             ?? throw BusinessError.Oh(CommonErrorCode.RecordNotFound, new { businessKey, });
                EnsureSourceAppScope(entity.AppId, appId, "公告");
                break;
            }

            case LiveOpsSourceKind.MailCampaign:
                // 邮件走既有 Campaign 链路（publish_mail_campaign），不参与灰度编排（OnlineConfigRolloutKind 无 Mail 成员）。
                throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed,
                    new { source, message = "邮件来源不参与灰度编排，请经统一下发或既有 Campaign 发布链路", });

            default:
                throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { source, });
        }
    }

    /// <summary>
    /// 统一构造 Online 调用（作用域 Factory 校验 + 命令 POST）；集中收口保证入口校验序在所有来源上一致。
    /// </summary>
    private async Task<OnlineLiveOpsResponse> PostOnlineAsync(string action, object payload, OnlineLiveOpsDispatchRequest input)
    {
        var scope = await _scopeFactory.CreateAsync(input.AppId, input.ServerId, action + ":" + input.BusinessKey);
        return await _onlineServerClient.PostAsync<OnlineLiveOpsResponse>(action, payload, scope);
    }

    /// <summary>
    /// 来源事实行应用归属校验：来源行 AppId 必须与下发目标 AppId 一致（RemoteConfig 租户默认行 AppId 为 null/0 时对任意本租户应用可用）。
    /// </summary>
    private static void EnsureSourceAppScope(long? sourceAppId, long targetAppId, string sourceName)
    {
        var resolved = sourceAppId ?? 0;
        if (resolved != 0 && resolved != targetAppId)
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed,
                new { sourceAppId = resolved, targetAppId, message = sourceName + "记录不属于目标应用", });
        }
    }

    /// <summary>
    /// Rollout 通道回执映射。
    /// </summary>
    private static OnlineConfigRolloutDispatchResponse ToRolloutResult(string action, OnlineLiveOpsResponse ack)
    {
        return new OnlineConfigRolloutDispatchResponse
        {
            OnlineAction = action,
            ConfirmVersion = ack.ConfirmVersion,
            Message = ack.Message,
            DispatchedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        };
    }
}
