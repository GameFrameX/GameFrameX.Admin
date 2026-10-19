using GameFrameX.Application.Game.Online.Console;
using GameFrameX.Application.Game.Online.Console.Dto;
using GameFrameX.Application.Game.ServerManager.Online;
using GameFrameX.Core.Tests.Online.Asset;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using SqlSugar;

namespace GameFrameX.Core.Tests.Online.Console;

// C344 / GFX-760（vault:C9 S8.1）：在线总览快照 Service 契约自检。
// 覆盖：action 名与作用域注入、快照字段透传（Admin 零聚合零推断）、SnapshotNote 口径说明、跨租户越界前置拒绝。
public sealed class OnlineOverviewQueryServiceTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"online_overview_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _scopeOwner;
    private readonly SqlSugarScopeProvider _db;

    public OnlineOverviewQueryServiceTests()
    {
        var config = new ConnectionConfig
        {
            ConfigId = "online-overview",
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _scopeOwner = new SqlSugarScope(new List<ConnectionConfig> { config });
        _db = _scopeOwner.GetConnectionScope("online-overview");
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(
            typeof(TenantGameAppEntity),
            typeof(TenantGameAreaEntity));
    }

    /// <summary>
    /// 快照透传：action=query_online_overview，作用域三元组经 scope 注入（线缆请求体为空），
    /// 快照字段原值往返 + SnapshotNote 注明「运行态快照非权威状态」。
    /// </summary>
    [Fact]
    public async Task SnapshotAsync_PassesScope_AndReturnsSnapshotWithNote()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        fake.SetPayload(new OnlineOverviewQueryResultResponse
        {
            OnlinePlayerCount = 1234,
            SessionCount = 1500,
            MatchCount = 88,
            ReconnectRate = 0.025,
            QueueSummaries = new List<OnlineOverviewQueueSummaryResponse>
            {
                new() { Mode = "ranked", Region = "cn-east", QueueDepth = 42, AverageWaitSeconds = 65, ThroughputPerMinute = 120, },
            },
            ServerTime = 1700000300,
        });
        var service = CreateService(fake);

        var response = await service.SnapshotAsync(new OnlineOverviewRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
        });

        var call = Assert.Single(fake.Calls);
        Assert.Equal("query_online_overview", call.Action);
        Assert.Equal(OnlineAssetTestSupport.Area1001, call.ServerId);
        var body = Assert.IsType<OnlineOverviewQueryRequest>(call.Request);

        Assert.Equal(1234, response.OnlinePlayerCount);
        Assert.Equal(1500, response.SessionCount);
        Assert.Equal(88, response.MatchCount);
        Assert.Equal(0.025, response.ReconnectRate);
        var queue = Assert.Single(response.QueueSummaries);
        Assert.Equal("ranked", queue.Mode);
        Assert.Equal(42, queue.QueueDepth);
        Assert.Equal(1700000300, response.ServerTime);
        Assert.Contains("非权威状态", response.SnapshotNote);
    }

    /// <summary>
    /// 越界前置拒绝：AppId 不归属当前租户 → ScopeDenied，Online 调用零次。
    /// </summary>
    [Fact]
    public async Task SnapshotAsync_RejectsCrossTenant_BeforeAnyOnlineCall()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        var service = CreateService(fake);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.SnapshotAsync(new OnlineOverviewRequest
        {
            AppId = 9999,
            ServerId = OnlineAssetTestSupport.Area1001,
        }));

        Assert.Equal((int)OnlineErrorCode.ScopeDenied, exception.Code);
        Assert.Empty(fake.Calls);
    }

    private OnlineOverviewQueryService CreateService(IOnlineServerClient onlineServerClient)
    {
        var factory = new OnlineScopeContextFactory(OnlineAssetTestSupport.CreateUserContext(), OnlineAssetTestSupport.Repository<TenantGameAppEntity>(_db), OnlineAssetTestSupport.Repository<TenantGameAreaEntity>(_db));
        return new OnlineOverviewQueryService(onlineServerClient, factory);
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
