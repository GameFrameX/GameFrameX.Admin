using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using GameFrameX.Application.Game.Online.LiveOps.Dispatch;
using GameFrameX.Application.Game.Online.LiveOps.Dispatch.Dto;
using GameFrameX.Application.Game.ServerManager.Online;
using GameFrameX.Core.Const;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Service.User;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Game.Mail;
using GameFrameX.Entity.Game.Online;
using GameFrameX.Entity.Tenant.Device;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Entity.Tenant.Game.Announcement;
using GameFrameX.Entity.Tenant.Game.Online;
using GameFrameX.Entity.Tenant.Game.RemoteConfig;
using GameFrameX.Foundation.Orm.Entity;
using Microsoft.AspNetCore.Http;
using SqlSugar;

namespace GameFrameX.Core.Tests.Online.LiveOps;

// C343 / GFX-755（vault:C8 S7.7）：统一下发服务行为测试（mock-first：fake IOnlineServerClient，联调延后）。
// 覆盖：四类配置来源路由到冻结命令名 + 载荷键断言、越枚举拒绝、来源不存在拒绝、应用归属不符拒绝、
// 邮件方案C（Published 同目标合并返回首次回执 / Draft 拒绝并指引 / ServerId≠AreaId 拒绝 / 合并路径不发起 Online 调用）、
// 分群同步通道回执映射。作用域走真实 OnlineScopeContextFactory（含租户归属校验）。
public sealed class OnlineLiveOpsDispatchServiceTests : IDisposable
{
    private const long TenantA = 1001;
    private const long App100 = 100;
    private const long App200 = 200;
    private const long Area1001 = 2001; // 归属 App100

    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"online_liveops_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _scopeOwner;
    private readonly SqlSugarScopeProvider _db;
    private readonly RecordingOnlineClient _client = new RecordingOnlineClient();

    public OnlineLiveOpsDispatchServiceTests()
    {
        // 专属 ConfigId（同 OnlineScopeContextFactoryTests 范式）：静态实体缓存与其它 fixture 隔离。
        var config = new ConnectionConfig
        {
            ConfigId = "online-liveops",
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _scopeOwner = new SqlSugarScope(new List<ConnectionConfig> { config });
        _db = _scopeOwner.GetConnectionScope("online-liveops");
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(
            typeof(TenantGameAppEntity),
            typeof(TenantGameAreaEntity),
            typeof(TenantGameAppRemoteConfigEntity),
            typeof(TenantGameAppDeviceGroupEntity),
            typeof(TenantGameTimeJobEntity),
            typeof(TenantGameAnnouncementEntity),
            typeof(TenantGameMailCampaignEntity));
    }

    /// <summary>
    /// 四类配置来源路由：来源 → 冻结命令名 + 业务主键进载荷 + 回执 ConfirmVersion 透传。
    /// </summary>
    [Fact]
    public async Task DispatchAsync_RoutesConfigSourcesToFrozenActions()
    {
        await SeedAsync();
        var service = CreateService();

        var cases = new[]
        {
            (Source: LiveOpsSourceKind.RemoteConfig, Key: 9001L, Action: "publishRemoteConfig", PayloadKey: "ConfigId"),
            (Source: LiveOpsSourceKind.DeviceGroup, Key: 9002L, Action: "syncDeviceGroup", PayloadKey: "DeviceGroupId"),
            (Source: LiveOpsSourceKind.ScheduledTask, Key: 9003L, Action: "syncScheduledTask", PayloadKey: "JobId"),
            (Source: LiveOpsSourceKind.Announcement, Key: 9004L, Action: "pushAnnouncement", PayloadKey: "AnnouncementId"),
        };

        foreach (var testCase in cases)
        {
            var response = await service.DispatchAsync(NewRequest(testCase.Source, testCase.Key));

            Assert.Equal(testCase.Action, response.DispatchChannel);
            Assert.Equal("cv-" + testCase.Action, response.OnlineConfirmVersion);
            Assert.False(response.Merged);

            var call = _client.Calls.Single(u => u.Action == testCase.Action);
            var payloadValue = call.PayloadProperty(testCase.PayloadKey);
            Assert.Equal(testCase.Key, payloadValue);
        }
    }

    /// <summary>
    /// 越枚举值（伪造 99）：入口即拒绝（ParameterValidationFailed），不落到任何通道。
    /// </summary>
    [Fact]
    public async Task DispatchAsync_RejectsUndefinedSource()
    {
        await SeedAsync();
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.DispatchAsync(NewRequest((LiveOpsSourceKind)99, 9001)));

        Assert.Equal(20000002, exception.Code);
        Assert.Empty(_client.Calls);
    }

    /// <summary>
    /// 来源记录不存在：RecordNotFound，不发起 Online 调用。
    /// </summary>
    [Fact]
    public async Task DispatchAsync_RejectsMissingSourceRecord()
    {
        await SeedAsync();
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.DispatchAsync(NewRequest(LiveOpsSourceKind.RemoteConfig, 424242)));

        Assert.Equal(20000003, exception.Code);
        Assert.Empty(_client.Calls);
    }

    /// <summary>
    /// 来源记录归属其它应用（AppId 不一致）：拒绝，杜绝跨应用误下发。
    /// </summary>
    [Fact]
    public async Task DispatchAsync_RejectsSourceFromOtherApp()
    {
        await SeedAsync(); // 公告 9005 归属 App200
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.DispatchAsync(NewRequest(LiveOpsSourceKind.Announcement, 9005)));

        Assert.Equal(20000002, exception.Code);
        Assert.Empty(_client.Calls);
    }

    /// <summary>
    /// 邮件方案C——Published 且同目标（ServerId == AreaId）：合并返回首次发布回执，不发起任何 Online 调用。
    /// </summary>
    [Fact]
    public async Task DispatchAsync_MailPublished_MergesFirstReceiptWithoutOnlineCall()
    {
        await SeedAsync(); // Campaign 9101：Published，Area1001，PublishVersion=3，ServerCampaignId=77
        var service = CreateService();

        var response = await service.DispatchAsync(NewRequest(LiveOpsSourceKind.MailCampaign, 9101));

        Assert.Equal("publish_mail_campaign", response.DispatchChannel);
        Assert.True(response.Merged);
        Assert.Equal("3", response.OnlineConfirmVersion);
        Assert.Empty(_client.Calls); // 合并路径零调用：发布动作由既有链路完成且幂等
    }

    /// <summary>
    /// 邮件方案C——Draft：状态机拒绝并指引走既有发布链路。
    /// </summary>
    [Fact]
    public async Task DispatchAsync_MailDraft_RejectedWithGuidance()
    {
        await SeedAsync(); // Campaign 9102：Draft
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.DispatchAsync(NewRequest(LiveOpsSourceKind.MailCampaign, 9102)));

        Assert.Contains("publish", exception.Message); // 指引文案
        Assert.Empty(_client.Calls);
    }

    /// <summary>
    /// 邮件方案C——ServerId 与 Campaign AreaId 不一致：拒绝（发布面向归属区服，不允许异区合并回执）。
    /// </summary>
    [Fact]
    public async Task DispatchAsync_MailServerMismatch_Rejected()
    {
        await SeedAsync(); // Campaign 9101 归属 Area1001，请求打 Area9999
        var service = CreateService();

        var input = NewRequest(LiveOpsSourceKind.MailCampaign, 9101);
        input.ServerId = 9999;
        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.DispatchAsync(input));

        Assert.Equal(20000002, exception.Code);
        Assert.Empty(_client.Calls);
    }

    /// <summary>
    /// 分群同步通道：载荷携带定义快照（Name/Conditions/Online 分群标识），Online 分配的 SegmentId 回传。
    /// </summary>
    [Fact]
    public async Task SyncSegmentToOnlineAsync_MapsDefinitionAndReturnsOnlineSegmentId()
    {
        await SeedAsync();
        var service = CreateService();
        _client.RespondWith = action => new OnlineLiveOpsResponse { ConfirmVersion = "seg-cv", SegmentId = 501, };

        var segment = new TenantGameOnlinePlayerSegmentEntity
        {
            Id = 7001,
            AppId = App100,
            Name = "七日活跃",
            ConditionsJson = "[{\"Field\":\"activeDays\",\"Op\":\"gte\",\"Value\":\"7\"}]",
        };

        var result = await service.SyncSegmentToOnlineAsync(segment, Area1001, "测试同步");

        Assert.Equal("syncPlayerSegment", result.OnlineAction);
        Assert.Equal(501, result.OnlineSegmentId);
        Assert.Equal("seg-cv", result.ConfirmVersion);

        var call = _client.Calls.Single(u => u.Action == "syncPlayerSegment");
        Assert.Equal("七日活跃", call.PayloadProperty("Name"));
    }

    private OnlineLiveOpsDispatchRequest NewRequest(LiveOpsSourceKind source, long businessKey)
    {
        return new OnlineLiveOpsDispatchRequest
        {
            Source = source,
            BusinessKey = businessKey,
            AppId = App100,
            ServerId = Area1001,
            Reason = "unit-test",
        };
    }

    private OnlineLiveOpsDispatchService CreateService()
    {
        return new OnlineLiveOpsDispatchService(
            _client,
            new OnlineScopeContextFactory(CreateUserContext(TenantA), Repository<TenantGameAppEntity>(), Repository<TenantGameAreaEntity>()),
            Repository<TenantGameAppRemoteConfigEntity>(),
            Repository<TenantGameAppDeviceGroupEntity>(),
            Repository<TenantGameTimeJobEntity>(),
            Repository<TenantGameAnnouncementEntity>(),
            Repository<TenantGameMailCampaignEntity>());
    }

    private static CurrentUserContext CreateUserContext(long? tenantId)
    {
        var identity = new ClaimsIdentity("TestAuth");
        if (tenantId.HasValue)
        {
            identity.AddClaim(new Claim(ClaimConst.TenantId, tenantId.Value.ToString()));
        }

        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity), };
        var accessor = new HttpContextAccessor { HttpContext = httpContext, };
        return new CurrentUserContext(accessor);
    }

    private SqlSugarRepository<T> Repository<T>() where T : EntityBase, new()
    {
        return new SqlSugarRepository<T> { Context = _db, };
    }

    private async Task SeedAsync()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await _db.Insertable(new TenantGameAppEntity { Id = App100, Name = "app-100", CreatedTime = now, IsDeleted = false, }).ExecuteCommandAsync();
        await _db.Insertable(new TenantGameAppEntity { Id = App200, Name = "app-200", CreatedTime = now, IsDeleted = false, }).ExecuteCommandAsync();
        await _db.Insertable(NewArea(Area1001, App100, now)).ExecuteCommandAsync();

        // 四类配置来源各一行（归属 App100）；公告 9005 归属 App200（构造跨应用场景）
        await _db.Insertable(FillStrings(new TenantGameAppRemoteConfigEntity { Id = 9001, AppId = App100 })).ExecuteCommandAsync();
        await _db.Insertable(FillStrings(new TenantGameAppDeviceGroupEntity { Id = 9002, AppId = App100 })).ExecuteCommandAsync();
        await _db.Insertable(FillStrings(new TenantGameTimeJobEntity { Id = 9003, AppId = App100 })).ExecuteCommandAsync();
        await _db.Insertable(FillStrings(new TenantGameAnnouncementEntity { Id = 9004, AppId = App100 })).ExecuteCommandAsync();
        await _db.Insertable(FillStrings(new TenantGameAnnouncementEntity { Id = 9005, AppId = App200 })).ExecuteCommandAsync();

        // 邮件方案C 场景：9101 Published（Area1001 / v3 / CampaignId 77），9102 Draft
        await _db.Insertable(FillStrings(new TenantGameMailCampaignEntity
        {
            Id = 9101,
            AppId = App100,
            AreaId = Area1001,
            Name = "campaign-published",
            PublishStatus = MailCampaignPublishStatus.Published,
            ServerCampaignId = 77,
            PublishVersion = 3,
            CreatedTime = now,
            IsDeleted = false,
        })).ExecuteCommandAsync();
        await _db.Insertable(FillStrings(new TenantGameMailCampaignEntity
        {
            Id = 9102,
            AppId = App100,
            AreaId = Area1001,
            Name = "campaign-draft",
            PublishStatus = MailCampaignPublishStatus.Draft,
            CreatedTime = now,
            IsDeleted = false,
        })).ExecuteCommandAsync();
    }

    private static TenantGameAreaEntity NewArea(long id, long gameAppId, long now)
    {
        return FillStrings(new TenantGameAreaEntity
        {
            Id = id,
            GameAppId = gameAppId,
            CreatedTime = now,
            IsDeleted = false,
        });
    }

    /// <summary>
    /// 反射填充全部 string 属性为空串（绕开实体 NOT NULL 列的种子样板；测试只关心 Id/AppId/状态字段）。
    /// </summary>
    private static T FillStrings<T>(T entity) where T : class
    {
        foreach (var property in typeof(T).GetProperties())
        {
            if (property.CanWrite && property.PropertyType == typeof(string))
            {
                property.SetValue(entity, string.Empty);
            }
        }

        return entity;
    }

    private sealed class RecordingOnlineClient : IOnlineServerClient
    {
        public List<RecordedCall> Calls { get; } = new List<RecordedCall>();

        public Func<string, OnlineLiveOpsResponse> RespondWith { get; set; } = action => new OnlineLiveOpsResponse { ConfirmVersion = "cv-" + action, };

        public Task<TResponse> PostAsync<TResponse>(string action, object request, OnlineScopeContext scope) where TResponse : class
        {
            Calls.Add(new RecordedCall(action, request, scope));
            return Task.FromResult((TResponse)(object)RespondWith(action));
        }

        public Task<TResponse> GetAsync<TResponse>(string action, OnlineScopeContext scope) where TResponse : class
        {
            throw new NotSupportedException("LiveOps 下发只走 POST 通道");
        }
    }

    private sealed class RecordedCall
    {
        public RecordedCall(string action, object payload, OnlineScopeContext scope)
        {
            Action = action;
            Payload = payload;
            Scope = scope;
        }

        public string Action { get; }

        public object Payload { get; }

        public OnlineScopeContext Scope { get; }

        /// <summary>
        /// 读取匿名载荷对象的属性值（作用域三元组由 OnlineServerClient 注入，不在业务载荷内）。
        /// </summary>
        public object? PayloadProperty(string name)
        {
            return Payload.GetType().GetProperty(name)?.GetValue(Payload);
        }
    }

    public void Dispose()
    {
        _scopeOwner.Dispose();
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }
}
