using GameFrameX.Application.Game.Online.Console;
using GameFrameX.Application.Game.Online.Console.Dto;
using GameFrameX.Application.Game.ServerManager.Online;
using GameFrameX.Core.Tests.Online.Asset;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Game.Online;
using GameFrameX.Entity.Log;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Entity.Tenant.Game.Online;
using SqlSugar;

namespace GameFrameX.Core.Tests.Online.Console;

// C344 / GFX-760（vault:C9 S8.2）：统一审计检索 Service 契约自检。
// 覆盖：三来源实时归并（Admin 操作日志 + Online 审计事件 + 域命令事实源）按 OccurredAt 倒序、
// 来源标注与作用域回填、五维过滤（目标对象/作用域）、keyset 游标翻页、Online 来源未锚定跳过、
// 脏游标拒绝、Online 来源失败原样透传（不软化缺源）。
public sealed class OnlineAuditSearchServiceTests : IDisposable
{
    private const long T1 = 1700000000;
    private const long T2 = 1700000100;
    private const long T3 = 1700000200;
    private const long T4 = 1700000300;
    private const long TB = 1700000250;

    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"audit_search_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _scopeOwner;
    private readonly SqlSugarScopeProvider _db;

    public OnlineAuditSearchServiceTests()
    {
        var config = new ConnectionConfig
        {
            ConfigId = "online-audit-search",
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _scopeOwner = new SqlSugarScope(new List<ConnectionConfig> { config });
        _db = _scopeOwner.GetConnectionScope("online-audit-search");
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(
            typeof(TenantGameAppEntity),
            typeof(TenantGameAreaEntity),
            typeof(TenantLogOperationEntity),
            typeof(TenantGamePenaltyEntity),
            typeof(TenantGameOnlineConfigRolloutEntity));
    }

    /// <summary>
    /// 三来源归并（作用域锚定）：按 OccurredAt 倒序混排，来源标注正确；
    /// Online 事件行回填查询锚定作用域；无作用域提取值的操作日志行（AppId=null）被作用域过滤排除。
    /// </summary>
    [Fact]
    public async Task PageAsync_MergesThreeSources_InOccurredAtOrder()
    {
        await SeedAllSourcesAsync();
        var fake = BuildOnlineEventFake();
        var service = CreateService(fake);

        var response = await service.PageAsync(new PageOnlineAuditSearchRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
        });

        Assert.Equal(4, response.Items.Count);
        // 倒序：opLog1(T4) > online 事件(TB) > rollout(T3) > penalty(T2)；opLog2 无作用域提取值被排除。
        Assert.Equal(T4, response.Items[0].OccurredAt);
        Assert.Equal(TB, response.Items[1].OccurredAt);
        Assert.Equal(T3, response.Items[2].OccurredAt);
        Assert.Equal(T2, response.Items[3].OccurredAt);

        Assert.Equal(OnlineAuditSourceKind.AdminOperation, response.Items[0].Source);
        Assert.Equal("admin-op", response.Items[0].OperatorIdentifier);
        Assert.Equal("player-9", response.Items[0].TargetPlayerIdentifier);
        Assert.Equal(OnlineAssetTestSupport.App100, response.Items[0].AppId);
        Assert.Equal(OnlineAssetTestSupport.Area1001, response.Items[0].ServerId);
        Assert.Equal("OnlinePenaltyLinkCommand.Execute", response.Items[0].CommandType);

        Assert.Equal(OnlineAuditSourceKind.OnlineAuditEvent, response.Items[1].Source);
        Assert.Equal(OnlineAssetTestSupport.App100, response.Items[1].AppId);
        Assert.Equal(OnlineAssetTestSupport.Area1001, response.Items[1].ServerId);
        Assert.Equal("asset.grant", response.Items[1].CommandType);

        Assert.Equal(OnlineAuditSourceKind.DomainCommandRecord, response.Items[2].Source);
        Assert.Equal("configRollout.RemoteConfig", response.Items[2].CommandType);

        Assert.Equal(OnlineAuditSourceKind.DomainCommandRecord, response.Items[3].Source);
        Assert.Equal("penalty.Mute", response.Items[3].CommandType);
        Assert.Equal("42", response.Items[3].OperatorIdentifier);
        Assert.Equal("player-9", response.Items[3].TargetPlayerIdentifier);

        Assert.False(response.HasMore);
        Assert.Null(response.NextCursor);
    }

    /// <summary>
    /// 五维过滤之目标对象：TargetPlayerId=player-9 → 只剩提取/直存该目标的行（opLog1 + Online 事件 + penalty），
    /// 无目标行（rollout）被排除。
    /// </summary>
    [Fact]
    public async Task PageAsync_FiltersByTargetPlayerId()
    {
        await SeedAllSourcesAsync();
        var fake = BuildOnlineEventFake();
        var service = CreateService(fake);

        var response = await service.PageAsync(new PageOnlineAuditSearchRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            TargetPlayerId = "player-9",
        });

        Assert.Equal(3, response.Items.Count);
        Assert.All(response.Items, entry => Assert.Equal("player-9", entry.TargetPlayerIdentifier));
        Assert.DoesNotContain(response.Items, entry => entry.CommandType == "configRollout.RemoteConfig");
    }

    /// <summary>
    /// keyset 游标翻页：PageSize=2 首页取最新两行并给出 NextCursor=末行 OccurredAt；
    /// 第二页全部行 OccurredAt 严格小于 cursor，翻尽后 HasMore=false / NextCursor=null。
    /// </summary>
    [Fact]
    public async Task PageAsync_KeysetCursor_PagesForward()
    {
        await SeedAllSourcesAsync();
        var fake = BuildOnlineEventFake();
        var service = CreateService(fake);

        var firstPage = await service.PageAsync(new PageOnlineAuditSearchRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            PageSize = 2,
        });
        Assert.Equal(2, firstPage.Items.Count);
        Assert.Equal(T4, firstPage.Items[0].OccurredAt);
        Assert.Equal(TB, firstPage.Items[1].OccurredAt);
        Assert.True(firstPage.HasMore);
        Assert.Equal(TB.ToString(), firstPage.NextCursor);

        var secondPage = await service.PageAsync(new PageOnlineAuditSearchRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            PageSize = 2,
            Cursor = firstPage.NextCursor,
        });
        Assert.Equal(2, secondPage.Items.Count);
        Assert.Equal(T3, secondPage.Items[0].OccurredAt);
        Assert.Equal(T2, secondPage.Items[1].OccurredAt);
        Assert.All(secondPage.Items, entry => Assert.True(entry.OccurredAt < TB));
        Assert.False(secondPage.HasMore);
        Assert.Null(secondPage.NextCursor);
    }

    /// <summary>
    /// Online 来源参与前置：未锚定 AppId+ServerId 时该来源静默跳过（零 Online 调用），
    /// 本地两来源仍全量检索（含无作用域提取值的操作日志行）。
    /// </summary>
    [Fact]
    public async Task PageAsync_SkipsOnlineSource_WhenScopeMissing()
    {
        await SeedAllSourcesAsync();
        var fake = BuildOnlineEventFake();
        var service = CreateService(fake);

        var response = await service.PageAsync(new PageOnlineAuditSearchRequest());

        Assert.Empty(fake.Calls);
        Assert.Equal(4, response.Items.Count);
        Assert.Equal(T4, response.Items[0].OccurredAt);
        Assert.Equal(T3, response.Items[1].OccurredAt);
        Assert.Equal(T2, response.Items[2].OccurredAt);
        Assert.Equal(T1, response.Items[3].OccurredAt);
        Assert.Equal(OnlineAuditSourceKind.AdminOperation, response.Items[3].Source);
        Assert.Equal("gm-2", response.Items[3].OperatorIdentifier);
    }

    /// <summary>
    /// 脏游标（非数字）按参数校验拒绝——静默当首页会翻页错乱。
    /// </summary>
    [Fact]
    public async Task PageAsync_RejectsDirtyCursor()
    {
        await SeedAllSourcesAsync();
        var fake = BuildOnlineEventFake();
        var service = CreateService(fake);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.PageAsync(new PageOnlineAuditSearchRequest
        {
            Cursor = "not-a-number",
        }));

        Assert.Equal((int)CommonErrorCode.ParameterValidationFailed, exception.Code);
    }

    /// <summary>
    /// Online 来源失败原样透传：审计检索宁可失败不可静默缺源。
    /// </summary>
    [Fact]
    public async Task PageAsync_PropagatesOnlineSourceFailure()
    {
        await SeedAllSourcesAsync();
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        fake.FailOn("query_online_audit_events", new BusinessException((global::System.Enum)OnlineErrorCode.ScopeDenied, "online source failed"));
        var service = CreateService(fake);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.PageAsync(new PageOnlineAuditSearchRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
        }));

        Assert.Equal((int)OnlineErrorCode.ScopeDenied, exception.Code);
        Assert.Single(fake.Calls);
    }

    private static OnlineAssetTestSupport.FakeOnlineServerClient BuildOnlineEventFake()
    {
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        fake.SetPayload(new OnlineAuditEventQueryResultResponse
        {
            Items = new List<OnlineAuditEventEntryResponse>
            {
                new()
                {
                    EventId = "oe-1",
                    CommandType = "asset.grant",
                    OperatorId = "op-77",
                    TargetPlayerId = "player-9",
                    OccurredAt = TB,
                    Summary = "人工发放",
                    CorrelationId = "corr-b",
                },
            },
        });
        return fake;
    }

    private async Task SeedAllSourcesAsync()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);

        await _db.Insertable(new TenantLogOperationEntity
        {
            Id = 1,
            HttpMethod = "POST",
            RequestUrl = "/api/onlinePenaltyLinkCommand/execute",
            RequestParam = $"{{\"input\":{{\"appId\":{OnlineAssetTestSupport.App100},\"serverId\":{OnlineAssetTestSupport.Area1001},\"playerId\":\"player-9\",\"ticketId\":\"t-77\"}}}}",
            ControllerName = "OnlinePenaltyLinkCommand",
            ActionName = "Execute",
            DisplayTitle = "处罚联动执行",
            Account = "admin-op",
            RealName = "管理员甲",
            LogDateTime = T4,
            CreatedTime = T4,
            Message = "ok",
            TraceId = "trace-1",
        }).ExecuteCommandAsync();

        await _db.Insertable(new TenantLogOperationEntity
        {
            Id = 2,
            HttpMethod = "POST",
            RequestUrl = "/api/gameApp/update",
            RequestParam = "{\"id\":7,\"name\":\"x\"}",
            ControllerName = "GameApp",
            ActionName = "Update",
            DisplayTitle = "更新应用",
            Account = "gm-2",
            LogDateTime = T1,
            CreatedTime = T1,
            Message = "ok",
        }).ExecuteCommandAsync();

        await _db.Insertable(new TenantGamePenaltyEntity
        {
            AppId = OnlineAssetTestSupport.App100,
            AreaId = OnlineAssetTestSupport.Area1001,
            TargetType = PenaltyTargetTypeEnum.Account,
            TargetId = "player-9",
            ActionType = PenaltyActionTypeEnum.Mute,
            Status = PenaltyStatusEnum.Active,
            Reason = "刷屏处罚",
            OperatorId = 42,
            CreatedTime = T2,
        }).ExecuteCommandAsync();

        await _db.Insertable(new TenantGameOnlineConfigRolloutEntity
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            ConfigKind = OnlineConfigRolloutKind.RemoteConfig,
            ConfigKey = "77",
            ConfigVersion = 3,
            SegmentId = 11,
            Percent = 50,
            Status = OnlineConfigRolloutStatus.Published,
            TargetVersion = 0,
            Reason = "灰度验证",
            DispatchOperator = "admin-op",
            PublishedTime = T3,
        }).ExecuteCommandAsync();
    }

    private OnlineAuditSearchService CreateService(IOnlineServerClient onlineServerClient)
    {
        var factory = new OnlineScopeContextFactory(OnlineAssetTestSupport.CreateUserContext(), OnlineAssetTestSupport.Repository<TenantGameAppEntity>(_db), OnlineAssetTestSupport.Repository<TenantGameAreaEntity>(_db));
        return new OnlineAuditSearchService(
            new SqlSugarRepository<TenantLogOperationEntity> { Context = _db, },
            OnlineAssetTestSupport.Repository<TenantGamePenaltyEntity>(_db),
            OnlineAssetTestSupport.Repository<TenantGameOnlineConfigRolloutEntity>(_db),
            onlineServerClient,
            factory);
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
