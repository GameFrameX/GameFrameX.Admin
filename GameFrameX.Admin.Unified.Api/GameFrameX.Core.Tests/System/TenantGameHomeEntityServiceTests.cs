using System.Net;
using GameFrameX.Application.Game.Home.GameHome;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Cache;
using GameFrameX.Core.Option;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;
using SqlSugar;

namespace GameFrameX.Core.Tests.System;

// C175：GetOnlineAsync 从全局 GameServerOptions.HttpRootUrl 改为按启用区服的 Control 能力前缀聚合。
// C321：getOnline/overview/onlineTrend 经租户库连接路由查询；平台面（无租户上下文）抛 BusinessException 明确语义。
// 用 Sqlite 双库 SqlSugarScope 种子数据，伪造 HttpMessageHandler 验证聚合、降级、计数与趋势桶行为。
public class TenantGameHomeEntityServiceTests : IDisposable
{
    private const long TenantId = 1001L;
    private const string TenantConfigId = "1001";

    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"game_home_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _db;
    private readonly FakeTenantDbConnectionProvider _tenantDbProvider;

    public TenantGameHomeEntityServiceTests()
    {
        SqlSugarCache.Configure(new FakeCache());
        // AuthTestHost 判例：连接配置必须经 SqlSugarSetup.SetDbConfig 装配 ConfigureExternalServices
        // （实体列名/类型服务，[TenantTable] 实体前缀 mutate 也在此链路），否则 ORM 写读列映射不一致
        // （实测种子 ControlApiPrefix 落 NULL、读回空导致 Build 抛降级）。建表与种子统一走租户库 scope。
        var config = new DbConnectionConfig
        {
            ConfigId = TenantConfigId,
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
            IsAutoCloseConnection = true,
            DbSettings = new DbSettings { EnableInitDb = true, },
            TableSettings = new TableSettings { EnableInitTable = true, EnableIncreTable = false, },
            SeedSettings = new SeedSettings { EnableInitSeed = false, EnableIncreSeed = false, },
        };
        SqlSugarSetup.SetDbConfig(config);
        _db = new SqlSugarScope(new List<ConnectionConfig> { config, });
        var tenantScope = _db.GetConnectionScope(TenantConfigId);
        tenantScope.CodeFirst.InitTables<TenantGameAreaEntity>();
        tenantScope.CodeFirst.InitTables<TenantGameOrderEntity>();
        tenantScope.CodeFirst.InitTables<TenantGameUserEntity>();
        tenantScope.CodeFirst.InitTables<TenantGameHomeOnlineSampleEntity>();
        _tenantDbProvider = new FakeTenantDbConnectionProvider(_db, TenantConfigId);
    }

    private TenantGameHomeEntityService CreateService(HttpClient? httpClient = null)
    {
        var client = httpClient ?? new HttpClient(new StubHandler(_ => throw new InvalidOperationException("should not call")));
        return new TenantGameHomeEntityService(new StubHttpClientFactory(client), _tenantDbProvider);
    }



    [Fact]
    public async Task GetOnline_AggregatesAcrossEnabledAreasUsingControlPrefix()
    {
        await SeedAreaAsync(10001L, "http://game1.example", "game/api");
        await SeedAreaAsync(10002L, "http://game2.example", "game/api");
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("5"),
        });
        var service = CreateService(new HttpClient(handler));

        using (TenantOperationContext.UseTenant(TenantId))
        {
            var result = await service.GetOnlineAsync();

            Assert.Equal(10, result.Data.OnlineCount);
            Assert.Equal(2, handler.Requests.Count);
            Assert.All(handler.Requests, u => Assert.EndsWith("/game/api/GetOnlinePlayer", u.AbsoluteUri));
        }
    }

    [Fact]
    public async Task GetOnline_ReturnsZeroWhenNoEnabledArea()
    {
        var handler = new StubHandler(_ => throw new InvalidOperationException("should not call"));
        var service = CreateService(new HttpClient(handler));

        using (TenantOperationContext.UseTenant(TenantId))
        {
            var result = await service.GetOnlineAsync();

            Assert.Equal(0, result.Data.OnlineCount);
            Assert.Empty(handler.Requests);
        }
    }

    [Fact]
    public async Task GetOnline_SkipsAreaWithoutControlPrefixAndDegradesToZero()
    {
        // 启用区服但 ControlApiPrefix 为空：构造器抛异常，聚合器捕获后跳过该区服，按 0 计
        await SeedAreaAsync(10001L, "http://game1.example", string.Empty);
        var handler = new StubHandler(_ => throw new InvalidOperationException("should not call without Control prefix"));
        var service = CreateService(new HttpClient(handler));

        using (TenantOperationContext.UseTenant(TenantId))
        {
            var result = await service.GetOnlineAsync();

            Assert.Equal(0, result.Data.OnlineCount);
            Assert.Empty(handler.Requests);
        }
    }

    [Fact]
    public async Task GetOnline_DegradesToZeroWhenGameServerUnreachable()
    {
        await SeedAreaAsync(10001L, "http://game1.example", "game/api");
        var handler = new StubHandler(_ => throw new HttpRequestException("connection refused"));
        var service = CreateService(new HttpClient(handler));

        using (TenantOperationContext.UseTenant(TenantId))
        {
            var result = await service.GetOnlineAsync();

            Assert.Equal(0, result.Data.OnlineCount);
        }
    }

    [Fact]
    public async Task GetOnline_ThrowsBusinessErrorOnPlatformContext()
    {
        // C321：平台面（SuperAdmin 无目标租户）不落到主库查租户表触发 42P01 裸 500，而是抛明确业务错误
        var service = CreateService();

        await Assert.ThrowsAsync<BusinessException>(() => service.GetOnlineAsync());
    }

    [Fact]
    public async Task Overview_CountsOrdersPlayersAndAreasByStatus()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        // 今日订单 2 条 + 昨日订单 1 条（NotShipped，计入待发货待办但不计今日）
        await SeedOrderAsync(20001L, now - 60, OrderStatusEnum.Completed);
        await SeedOrderAsync(20002L, now - 120, OrderStatusEnum.NotShipped);
        await SeedOrderAsync(20003L, now - 86400, OrderStatusEnum.NotShipped);
        await SeedOrderAsync(20004L, now - 90, OrderStatusEnum.InitiateRefund);
        await SeedOrderAsync(20005L, now - 86400, OrderStatusEnum.RefundAgreement);
        await SeedOrderAsync(20006L, now - 30, OrderStatusEnum.Unpaid);
        // 玩家：今日新增 2 + 昨日 1
        await SeedPlayerAsync(30001L, now - 60);
        await SeedPlayerAsync(30002L, now - 120);
        await SeedPlayerAsync(30003L, now - 86400);
        // 区服：Online 1 + Maintenance 1 + Offline 1（均无管理地址：聚合在线查询过滤后为空，不发起 HTTP）
        await SeedAreaAsync(10001L, "", "", GameServerStatus.Online);
        await SeedAreaAsync(10002L, "", "", GameServerStatus.Maintenance);
        await SeedAreaAsync(10003L, "", "", GameServerStatus.Offline);
        var service = CreateService();

        using (TenantOperationContext.UseTenant(TenantId))
        {
            var result = await service.OverviewAsync();

            var overview = result.Data;
            Assert.Equal(4, overview.TodayOrderCount);
            Assert.Equal(2, overview.PendingShipmentCount);
            Assert.Equal(2, overview.RefundingCount);
            Assert.Equal(3, overview.PlayerTotalCount);
            Assert.Equal(2, overview.PlayerTodayNewCount);
            Assert.Equal(3, overview.AreaTotalCount);
            Assert.Equal(1, overview.AreaRunningCount);
            Assert.Equal(1, overview.AreaMaintenanceCount);
            Assert.Equal(0, overview.OnlineCount);
        }
    }

    [Fact]
    public async Task Overview_ThrowsBusinessErrorOnPlatformContext()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<BusinessException>(() => service.OverviewAsync());
    }

    [Fact]
    public async Task OnlineTrend_FillsHourBucketsWithLastSampleAndNullForGaps()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var currentHour = now / 3600 * 3600;
        // 当前小时内两个 15 分钟桶：后写覆盖前写（桶内取最后采样）
        await SeedSampleAsync(currentHour, 12);
        await SeedSampleAsync(currentHour + 1800, 15);
        // 3 小时前的整点桶：正常回读
        await SeedSampleAsync(currentHour - 3 * 3600 + 900, 9);
        var service = CreateService();

        using (TenantOperationContext.UseTenant(TenantId))
        {
            var result = await service.OnlineTrendAsync();

            var points = result.Data.Points;
            Assert.Equal(24, points.Count);
            Assert.Equal(currentHour - 23 * 3600, points[0].BucketTime);
            Assert.Equal(currentHour, points[23].BucketTime);
            // 当前小时桶取最后采样值
            Assert.Equal(15, points[23].OnlineCount);
            // 3 小时前的桶有采样
            Assert.Equal(9, points[20].OnlineCount);
            // 无采样桶为 null（相邻两桶）
            Assert.Null(points[22].OnlineCount);
            Assert.Null(points[19].OnlineCount);
        }
    }

    [Fact]
    public async Task OnlineTrend_ThrowsBusinessErrorOnPlatformContext()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<BusinessException>(() => service.OnlineTrendAsync());
    }

    private async Task SeedAreaAsync(long id, string httpManageUrl, string controlPrefix, GameServerStatus serverStatus = GameServerStatus.Online)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await _db.GetConnectionScope(TenantConfigId).Insertable(new TenantGameAreaEntity
        {
            Id = id,
            TenantId = TenantId,
            CreatedId = 1,
            CreatedTime = now,
            CreatedName = "test",
            UpdateCount = 0,
            UpdateTime = now,
            UpdatedId = 1,
            UpdatedName = "test",
            IsDeleted = false,
            IsEnabled = true,
            DeleteTime = 0,
            DeletedId = 0,
            DeletedName = string.Empty,
            RowVersion = 0,
            Name = "area-" + id,
            Description = "test",
            HttpManageUrl = httpManageUrl,
            TcpSocketUrl = "tcp://test",
            WebSocketUrl = "wss://test",
            CurrentLoad = 0,
            CreationTime = now,
            LastMaintenanceTime = now,
            ServerVersion = "test",
            ServerType = "test",
            LocalizationKey = "test",
            AllowedPlayerProfile = "test",
            ServerEvents = "test",
            AccessControls = "test",
            ServerContainerId = "test",
            ServerParams = "test",
            AppVersion = "test",
            ControlApiPrefix = controlPrefix,
            ServerStatus = serverStatus,
        }).ExecuteCommandAsync();
    }

    private async Task SeedOrderAsync(long id, long createdTime, OrderStatusEnum status)
    {
        await _db.GetConnectionScope(TenantConfigId).Insertable(new TenantGameOrderEntity
        {
            Id = id,
            TenantId = TenantId,
            CreatedId = 1,
            CreatedTime = createdTime,
            CreatedName = "test",
            UpdateCount = 0,
            UpdateTime = createdTime,
            UpdatedId = 1,
            UpdatedName = "test",
            IsDeleted = false,
            IsEnabled = true,
            DeleteTime = 0,
            DeletedId = 0,
            DeletedName = string.Empty,
            RowVersion = 0,
            AppId = 100,
            Status = status,
            RoleId = "role-" + id,
            Amount = 100,
            Content = "test",
            AreaId = 1,
            OrderId = "order-" + id,
            Sku = 1,
            ProductId = "sku-" + id,
            PayChannel = "alipay",
            Channel = "test",
            SubChannel = string.Empty,
            Platform = "test",
            Country = string.Empty,
            IsSandbox = false,
            RoleRegisterTime = 0,
            AppleVerifiedTime = 0,
            RealMoney = 0,
            Level = 0,
            AdventureSuffix = string.Empty,
            AccountId = "account-" + id,
        }).ExecuteCommandAsync();
    }

    private async Task SeedPlayerAsync(long id, long createdTime)
    {
        await _db.GetConnectionScope(TenantConfigId).Insertable(new TenantGameUserEntity
        {
            Id = id,
            TenantId = TenantId,
            CreatedId = 1,
            CreatedTime = createdTime,
            CreatedName = "test",
            UpdateCount = 0,
            UpdateTime = createdTime,
            UpdatedId = 1,
            UpdatedName = "test",
            IsDeleted = false,
            IsEnabled = true,
            DeleteTime = 0,
            DeletedId = 0,
            DeletedName = string.Empty,
            RowVersion = 0,
            UserName = "user-" + id,
            Name = "user-" + id,
            Description = string.Empty,
            Password = "test",
            Email = string.Empty,
            EmailPassword = string.Empty,
            Status = GameUserStatus.Active,
            Phone = string.Empty,
            Avatar = string.Empty,
            GoogleId = string.Empty,
            AppleId = string.Empty,
            FacebookId = string.Empty,
            TwitterId = string.Empty,
            DiscordId = string.Empty,
            QqId = string.Empty,
            WeChatId = string.Empty,
            DeviceId = string.Empty,
        }).ExecuteCommandAsync();
    }

    private async Task SeedSampleAsync(long sampleTime, int onlineCount)
    {
        await _db.GetConnectionScope(TenantConfigId).Insertable(new TenantGameHomeOnlineSampleEntity
        {
            Id = 40000L + sampleTime % 100000,
            TenantId = TenantId,
            CreatedId = 0,
            CreatedTime = sampleTime,
            CreatedName = string.Empty,
            UpdateCount = 0,
            UpdateTime = sampleTime,
            UpdatedId = 0,
            UpdatedName = string.Empty,
            IsDeleted = false,
            IsEnabled = true,
            DeleteTime = 0,
            DeletedId = 0,
            DeletedName = string.Empty,
            RowVersion = 0,
            SampleTime = sampleTime,
            OnlineCount = onlineCount,
        }).ExecuteCommandAsync();
    }

    public void Dispose()
    {
        _db.Dispose();
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _httpClient;

        public StubHttpClientFactory(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public HttpClient CreateClient(string name)
        {
            return _httpClient;
        }
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _send;

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> send)
        {
            _send = send;
        }

        public List<Uri> Requests { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
#pragma warning disable CS8604 // 测试桩：测试用例均使用绝对 URL，RequestUri 不为 null
            Requests.Add(request.RequestUri);
#pragma warning restore CS8604
            return Task.FromResult(_send(request));
        }
    }
}
