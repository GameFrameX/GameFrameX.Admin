using GameFrameX.Application.Game.Online.Asset;
using GameFrameX.Application.Game.Online.Asset.Dto;
using GameFrameX.Application.Game.ServerManager.Online;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using SqlSugar;

namespace GameFrameX.Core.Tests.Online.Asset;

// C340 / GFX-752（vault:C4 S3.7）：资产审计查询服务契约自检。
// 覆盖：操作者/玩家/命令类型/时间窗过滤 + 游标透传、线缆响应映射、跨租户越界拒绝。
public sealed class OnlineAssetAuditQueryServiceTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"asset_audit_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _scopeOwner;
    private readonly SqlSugarScopeProvider _db;

    public OnlineAssetAuditQueryServiceTests()
    {
        var config = new ConnectionConfig
        {
            ConfigId = "online-asset-audit",
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _scopeOwner = new SqlSugarScope(new List<ConnectionConfig> { config });
        _db = _scopeOwner.GetConnectionScope("online-asset-audit");
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(
            typeof(TenantGameAppEntity),
            typeof(TenantGameAreaEntity));
    }

    /// <summary>
    /// 过滤条件透传 + 游标往返 + AuditEntry 字段映射。
    /// </summary>
    [Fact]
    public async Task PageAsync_PassesFiltersAndCursor_AndMapsAuditEntries()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        fake.SetPayload(new OnlineAssetAuditQueryResultResponse
        {
            Items = new List<OnlineAssetAuditEntryResponse>
            {
                new()
                {
                    TransactionId = "tx-1",
                    CommandType = "grant",
                    OperatorId = "op-1",
                    PlayerId = "player-1",
                    Reason = "manual",
                    BusinessOrderNumber = "BO-1",
                    OccurredAt = 1780000000,
                },
            },
            NextCursor = "audit-next",
            HasMore = true,
        });
        var service = CreateService(fake);

        var response = await service.PageAsync(new PageOnlineAssetAuditRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            OperatorId = "op-1",
            PlayerId = "player-1",
            CommandType = "grant",
            StartTime = 1780000000,
            EndTime = 1780000999,
            Cursor = "audit-prev",
            PageSize = 30,
        });

        var call = Assert.Single(fake.Calls);
        Assert.Equal("query_asset_audit", call.Action);
        Assert.Equal(OnlineAssetTestSupport.Area1001, call.ServerId);

        var body = Assert.IsType<OnlineAssetAuditQueryRequest>(call.Request);
        Assert.Equal("op-1", body.OperatorId);
        Assert.Equal("player-1", body.PlayerId);
        Assert.Equal("grant", body.CommandType);
        Assert.Equal("audit-prev", body.Cursor);
        Assert.Equal(30, body.PageSize);

        Assert.Equal("audit-next", response.NextCursor);
        Assert.True(response.HasMore);
        var entry = Assert.Single(response.Items);
        Assert.Equal("tx-1", entry.TransactionId);
        Assert.Equal("grant", entry.CommandType);
        Assert.Equal("op-1", entry.OperatorId);
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

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.PageAsync(new PageOnlineAssetAuditRequest
        {
            AppId = 9999,
            ServerId = OnlineAssetTestSupport.Area1001,
        }));

        Assert.Equal((int)OnlineErrorCode.ScopeDenied, exception.Code);
        Assert.Empty(fake.Calls);
    }

    private OnlineAssetAuditQueryService CreateService(IOnlineServerClient onlineServerClient)
    {
        var factory = new OnlineScopeContextFactory(OnlineAssetTestSupport.CreateUserContext(), OnlineAssetTestSupport.Repository<TenantGameAppEntity>(_db), OnlineAssetTestSupport.Repository<TenantGameAreaEntity>(_db));
        return new OnlineAssetAuditQueryService(onlineServerClient, factory);
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
