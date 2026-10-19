using GameFrameX.Application.Game.Online.Console;
using GameFrameX.Application.Game.Online.Console.Dto;
using GameFrameX.Application.Game.ServerManager.Online;
using GameFrameX.Core.Tests.Online.Asset;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using SqlSugar;

namespace GameFrameX.Core.Tests.Online.Console;

// C344 / GFX-760（vault:C9 S8.1）：玩家时间线查询 Service 契约自检。
// 覆盖：action 名与过滤/时间窗/游标透传、事件行信封原值往返、空事件空槽语义（空列表非错误）、跨租户越界前置拒绝。
public sealed class OnlinePlayerTimelineQueryServiceTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"player_timeline_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _scopeOwner;
    private readonly SqlSugarScopeProvider _db;

    public OnlinePlayerTimelineQueryServiceTests()
    {
        var config = new ConnectionConfig
        {
            ConfigId = "online-player-timeline",
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _scopeOwner = new SqlSugarScope(new List<ConnectionConfig> { config });
        _db = _scopeOwner.GetConnectionScope("online-player-timeline");
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(
            typeof(TenantGameAppEntity),
            typeof(TenantGameAreaEntity));
    }

    /// <summary>
    /// 过滤透传：PlayerId/EventGroup/时间窗/游标/页大小原值下推 Online；响应事件行与游标原值往返、PlayerId 回显。
    /// </summary>
    [Fact]
    public async Task QueryAsync_PassesFiltersAndReturnsEnvelopeRows()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        fake.SetPayload(new OnlinePlayerTimelineQueryResultResponse
        {
            Events = new List<OnlinePlayerTimelineEntryResponse>
            {
                new()
                {
                    EventId = "evt-1",
                    EventType = "penalty.applied",
                    Group = "Penalty",
                    OccurredAt = 1700000200,
                    Source = "online-social",
                    CorrelationId = "corr-1",
                    PayloadSummary = "mute 30m",
                },
            },
            NextCursor = "next-1",
            HasMore = true,
        });
        var service = CreateService(fake);

        var response = await service.QueryAsync(new OnlinePlayerTimelineRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            PlayerId = "player-9",
            EventGroup = OnlinePlayerTimelineEventGroup.Penalty,
            StartTime = 1700000000,
            EndTime = 1700000300,
            Cursor = "cursor-0",
            PageSize = 50,
        });

        var call = Assert.Single(fake.Calls);
        Assert.Equal("query_player_timeline", call.Action);
        var body = Assert.IsType<OnlinePlayerTimelineQueryRequest>(call.Request);
        Assert.Equal("player-9", body.PlayerId);
        Assert.Equal("Penalty", body.EventGroup);
        Assert.Equal(1700000000, body.StartTime);
        Assert.Equal(1700000300, body.EndTime);
        Assert.Equal("cursor-0", body.Cursor);
        Assert.Equal(50, body.PageSize);

        Assert.Equal("player-9", response.PlayerId);
        var entry = Assert.Single(response.Events);
        Assert.Equal("evt-1", entry.EventId);
        Assert.Equal("penalty.applied", entry.EventType);
        Assert.Equal("Penalty", entry.Group);
        Assert.Equal(1700000200, entry.OccurredAt);
        Assert.Equal("online-social", entry.Source);
        Assert.Equal("corr-1", entry.CorrelationId);
        Assert.Equal("mute 30m", entry.PayloadSummary);
        Assert.Equal("next-1", response.NextCursor);
        Assert.True(response.HasMore);
    }

    /// <summary>
    /// 空槽语义：Online 无事件 → Events 空列表（非 null 非错误），游标为空、HasMore=false。
    /// </summary>
    [Fact]
    public async Task QueryAsync_EmptyEvents_ReturnsEmptyListNotError()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        fake.SetPayload(new OnlinePlayerTimelineQueryResultResponse());
        var service = CreateService(fake);

        var response = await service.QueryAsync(new OnlinePlayerTimelineRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            PlayerId = "player-9",
        });

        Assert.NotNull(response.Events);
        Assert.Empty(response.Events);
        Assert.Null(response.NextCursor);
        Assert.False(response.HasMore);
    }

    /// <summary>
    /// 越界前置拒绝：AppId 不归属当前租户 → ScopeDenied，Online 调用零次。
    /// </summary>
    [Fact]
    public async Task QueryAsync_RejectsCrossTenant_BeforeAnyOnlineCall()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        var service = CreateService(fake);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.QueryAsync(new OnlinePlayerTimelineRequest
        {
            AppId = 9999,
            ServerId = OnlineAssetTestSupport.Area1001,
            PlayerId = "player-9",
        }));

        Assert.Equal((int)OnlineErrorCode.ScopeDenied, exception.Code);
        Assert.Empty(fake.Calls);
    }

    private OnlinePlayerTimelineQueryService CreateService(IOnlineServerClient onlineServerClient)
    {
        var factory = new OnlineScopeContextFactory(OnlineAssetTestSupport.CreateUserContext(), OnlineAssetTestSupport.Repository<TenantGameAppEntity>(_db), OnlineAssetTestSupport.Repository<TenantGameAreaEntity>(_db));
        return new OnlinePlayerTimelineQueryService(onlineServerClient, factory);
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
