using GameFrameX.Application.Game.Online.Asset;
using GameFrameX.Application.Game.Online.Asset.Dto;
using GameFrameX.Application.Game.ServerManager.Online;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using SqlSugar;

namespace GameFrameX.Core.Tests.Online.Asset;

// C340 / GFX-752（vault:C4 S3.7）：资产流水查询服务契约自检。
// 覆盖：过滤+游标透传、线缆响应 → Admin 响应映射（DTO 映射）、
// 作用域缺失 / 跨租户越界拒绝（越界拒绝在 scope factory 抛，fake zero calls）。
public sealed class OnlineAssetLedgerQueryServiceTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"asset_ledger_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _scopeOwner;
    private readonly SqlSugarScopeProvider _db;

    public OnlineAssetLedgerQueryServiceTests()
    {
        var config = new ConnectionConfig
        {
            ConfigId = "online-asset-ledger",
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _scopeOwner = new SqlSugarScope(new List<ConnectionConfig> { config });
        _db = _scopeOwner.GetConnectionScope("online-asset-ledger");
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(
            typeof(TenantGameAppEntity),
            typeof(TenantGameAreaEntity));
    }

    /// <summary>
    /// 过滤+游标透传：线缆请求体携带 PlayerId / CurrencyOrItemType / 时间窗 / Cursor / PageSize；
    /// 线缆响应 NextCursor + HasMore 透传至 Admin 响应。
    /// </summary>
    [Fact]
    public async Task PageAsync_PassesFiltersAndCursor_RoundTripsToAdminResponse()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        fake.SetPayload(new OnlineAssetLedgerQueryResultResponse
        {
            Items = new List<OnlineAssetLedgerEntryResponse>
            {
                new()
                {
                    TransactionId = "tx-1",
                    PlayerId = "player-1",
                    CurrencyOrItemType = "gold",
                    BeforeValue = 100,
                    AfterValue = 150,
                    Source = "Admin",
                    Reason = "manual grant",
                    BusinessOrderNumber = "BO-1",
                    OperatorId = "op-1",
                    OccurredAt = 1780000000,
                },
            },
            NextCursor = "next-cursor-token",
            HasMore = true,
        });
        var service = CreateService(fake);

        var response = await service.PageAsync(new PageOnlineAssetLedgerRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            PlayerId = "player-1",
            CurrencyOrItemType = "gold",
            StartTime = 1780000000,
            EndTime = 1780000999,
            Cursor = "prev-cursor",
            PageSize = 50,
        });

        var call = Assert.Single(fake.Calls);
        Assert.Equal("query_wallet_ledger", call.Action);
        Assert.Equal(OnlineAssetTestSupport.Area1001, call.ServerId);

        var body = Assert.IsType<OnlineAssetLedgerQueryRequest>(call.Request);
        Assert.Equal("player-1", body.PlayerId);
        Assert.Equal("gold", body.CurrencyOrItemType);
        Assert.Equal(1780000000, body.StartTime);
        Assert.Equal(1780000999, body.EndTime);
        Assert.Equal("prev-cursor", body.Cursor);
        Assert.Equal(50, body.PageSize);

        Assert.Equal("next-cursor-token", response.NextCursor);
        Assert.True(response.HasMore);
        var row = Assert.Single(response.Items);
        Assert.Equal("tx-1", row.TransactionId);
        Assert.Equal(100, row.BeforeValue);
        Assert.Equal(150, row.AfterValue);
    }

    /// <summary>
    /// 越界拒绝：AppId 不归属当前租户，scope factory 抛 ScopeDenied，Online 调用零次。
    /// </summary>
    [Fact]
    public async Task PageAsync_RejectsCrossTenantApp_BeforeAnyOnlineCall()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        var service = CreateService(fake);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.PageAsync(new PageOnlineAssetLedgerRequest
        {
            AppId = 9999,
            ServerId = OnlineAssetTestSupport.Area1001,
            PlayerId = "player-1",
        }));

        Assert.Equal((int)OnlineErrorCode.ScopeDenied, exception.Code);
        Assert.Empty(fake.Calls);
    }

    /// <summary>
    /// 作用域缺失：AppId=0 抛 ScopeMissing（32200005），Online 调用零次。
    /// </summary>
    [Fact]
    public async Task PageAsync_RejectsMissingScope()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        var service = CreateService(fake);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.PageAsync(new PageOnlineAssetLedgerRequest
        {
            AppId = 0,
            ServerId = OnlineAssetTestSupport.Area1001,
            PlayerId = "player-1",
        }));

        Assert.Equal((int)OnlineErrorCode.ScopeMissing, exception.Code);
        Assert.Empty(fake.Calls);
    }

    private OnlineAssetLedgerQueryService CreateService(IOnlineServerClient onlineServerClient)
    {
        var factory = new OnlineScopeContextFactory(OnlineAssetTestSupport.CreateUserContext(), OnlineAssetTestSupport.Repository<TenantGameAppEntity>(_db), OnlineAssetTestSupport.Repository<TenantGameAreaEntity>(_db));
        return new OnlineAssetLedgerQueryService(onlineServerClient, factory);
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
