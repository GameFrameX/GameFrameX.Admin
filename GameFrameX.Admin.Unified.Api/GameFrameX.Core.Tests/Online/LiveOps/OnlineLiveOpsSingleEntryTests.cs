using System.Reflection;
using GameFrameX.Application.Game.Announcement.GameAnnouncement;
using GameFrameX.Application.Game.App.GameAppRemoteConfig;
using GameFrameX.Application.Game.Mail.GameMailCampaign;
using GameFrameX.Application.Game.Online.LiveOps.ConfigRollout;
using GameFrameX.Application.Game.Online.LiveOps.Dispatch;
using GameFrameX.Application.Game.Online.LiveOps.PlayerSegment;
using GameFrameX.Application.Game.Online.RolloutSemanticMap;
using GameFrameX.Application.Game.ServerManager.Online;
using GameFrameX.Core.Api;
using GameFrameX.Entity.Game.Online;

namespace GameFrameX.Core.Tests.Online.LiveOps;

// C343 / GFX-755（vault:C8 S7.7）：LiveOps 统一下发单一入口守护测试。
// 覆盖：Application.Game 业务层唯一 IOnlineServerClient 注入点 = OnlineLiveOpsDispatchService（旁路直连即失败）、
// 七个 Online 命令名冻结、兄弟 Service 一律依赖 IOnlineLiveOpsDispatchService 转调、
// LiveOpsSourceKind 五值契约、两个新 Service 可被动态 API 路由发现（IAdminApiController）、
// 语义映射表边界与白名单唯一来源。纯反射无 DB。
public sealed class OnlineLiveOpsSingleEntryTests
{
    /// <summary>
    /// 单一入口铁律：LiveOps 域（GameFrameX.Application.Game.Online.LiveOps）内除 Dispatch 实现类自身外，
    /// 任何类都不得注入 IOnlineServerClient（旁路直连即失败）。
    /// 范围说明（2026-09-13 C340 rebase 收窄）：Asset/Social/Console/Match 观测域 Service 按各自 change 设计
    /// 直连 IOnlineServerClient（C339 基座的既定消费方式；Match 域为 C341 纯只读观测，不属于 LiveOps 写命令
    /// 统一下发范畴，其零持久化约束由 OnlineObservatoryNoPersistenceTests 独立守护），不在本断言范围；
    /// 三个既有运营域（RemoteConfig/Announcement/MailCampaign）的下发入口由 SiblingServices 断言锁定。
    /// </summary>
    [Fact]
    public void OnlyDispatchService_InjectsOnlineServerClient()
    {
        var offenders = new List<string>();
        foreach (var type in typeof(OnlineLiveOpsDispatchService).Assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface || type == typeof(OnlineLiveOpsDispatchService))
            {
                continue;
            }

            if (type.Namespace is null ||
                !type.Namespace.StartsWith("GameFrameX.Application.Game.Online.LiveOps", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (var constructor in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
            {
                if (constructor.GetParameters().Any(parameter => parameter.ParameterType == typeof(IOnlineServerClient)))
                {
                    offenders.Add(type.FullName!);
                }
            }
        }

        Assert.Empty(offenders);
    }

    /// <summary>
    /// Online 命令名冻结（change.md A0 契约对齐块）：常量值与路由表逐个锁定，改名须回 change 评审。
    /// </summary>
    [Fact]
    public void FrozenOnlineActionNames_AreExact()
    {
        Assert.Equal("publishRemoteConfig", OnlineLiveOpsDispatchService.ActionPublishRemoteConfig);
        Assert.Equal("syncDeviceGroup", OnlineLiveOpsDispatchService.ActionSyncDeviceGroup);
        Assert.Equal("syncScheduledTask", OnlineLiveOpsDispatchService.ActionSyncScheduledTask);
        Assert.Equal("pushAnnouncement", OnlineLiveOpsDispatchService.ActionPushAnnouncement);
        Assert.Equal("syncPlayerSegment", OnlineLiveOpsDispatchService.ActionSyncPlayerSegment);
        Assert.Equal("publishConfigRollout", OnlineLiveOpsDispatchService.ActionPublishConfigRollout);
        Assert.Equal("rollbackConfigRollout", OnlineLiveOpsDispatchService.ActionRollbackConfigRollout);
        Assert.Equal("publish_mail_campaign", OnlineLiveOpsDispatchService.ChannelPublishMailCampaign);
    }

    /// <summary>
    /// 兄弟 Service（两个新 Service + 三个既有 Service 的 dispatchOnline）一律依赖 IOnlineLiveOpsDispatchService，不得旁路。
    /// </summary>
    [Fact]
    public void SiblingServices_DependOnDispatchInterface()
    {
        var siblings = new[]
        {
            typeof(GameFrameX.Application.Game.Online.LiveOps.PlayerSegment.TenantGameOnlinePlayerSegmentService),
            typeof(TenantGameOnlineConfigRolloutService),
            typeof(TenantGameAppRemoteConfigService),
            typeof(TenantGameAnnouncementEntityService),
            typeof(TenantGameMailCampaignEntityService),
        };

        foreach (var sibling in siblings)
        {
            var depends = sibling.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .SelectMany(constructor => constructor.GetParameters())
                .Any(parameter => parameter.ParameterType == typeof(IOnlineLiveOpsDispatchService));
            Assert.True(depends, sibling.Name + " 必须依赖 IOnlineLiveOpsDispatchService（单一入口）");
        }
    }

    /// <summary>
    /// LiveOpsSourceKind 五值契约：成员名与数值锁定（新增来源须回 change 评审并同步本断言）。
    /// </summary>
    [Fact]
    public void LiveOpsSourceKind_ExactlyFiveFrozenValues()
    {
        var expected = new Dictionary<string, long>
        {
            [nameof(LiveOpsSourceKind.RemoteConfig)] = 1,
            [nameof(LiveOpsSourceKind.DeviceGroup)] = 2,
            [nameof(LiveOpsSourceKind.ScheduledTask)] = 3,
            [nameof(LiveOpsSourceKind.Announcement)] = 4,
            [nameof(LiveOpsSourceKind.MailCampaign)] = 5,
        };

        var actual = global::System.Enum.GetNames<LiveOpsSourceKind>()
            .ToDictionary(name => name, name => Convert.ToInt64(global::System.Enum.Parse<LiveOpsSourceKind>(name)));
        Assert.Equal(expected.Count, actual.Count);
        foreach (var pair in expected)
        {
            Assert.Equal(pair.Value, actual[pair.Key]);
        }
    }

    /// <summary>
    /// 新 Service 实现动态 API 路由标记（IAdminApiController），否则 AdminApiRouteRules 不会为其生成 /api/ 路由。
    /// </summary>
    [Fact]
    public void NewServices_AreAdminApiControllers()
    {
        Assert.Contains(typeof(IAdminApiController), typeof(OnlineLiveOpsDispatchService).GetInterfaces());
        Assert.Contains(typeof(IAdminApiController), typeof(TenantGameOnlineConfigRolloutService).GetInterfaces());
    }

    /// <summary>
    /// 语义映射表唯一来源：Percent 边界（1/100）与条件白名单（三字段 + 三操作符）锁定，散落数值即失败。
    /// </summary>
    [Fact]
    public void SemanticMap_BoundariesAndWhitelocks_AreFrozen()
    {
        Assert.Equal(1, OnlineRolloutSemanticMap.PublishPercentMin);
        Assert.Equal(100, OnlineRolloutSemanticMap.PublishPercentMax);
        Assert.Equal(new[] { "activeDays", "retentionDays", "totalPaymentAmount" }, OnlineRolloutSemanticMap.SegmentConditionFields.OrderBy(u => u).ToArray());
        Assert.Equal(new[] { "eq", "gte", "lte" }, OnlineRolloutSemanticMap.SegmentConditionOperators.OrderBy(u => u).ToArray());
    }
}
