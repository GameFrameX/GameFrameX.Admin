using GameFrameX.Application.Game.Online.Asset;
using GameFrameX.Application.Game.Online.Asset.Dto;
using GameFrameX.Application.Game.ServerManager.Online;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using SqlSugar;

// S3/C401 断链修复点
namespace GameFrameX.Core.Tests.Online.Asset;

// C340 / GFX-752（vault:C4 S3.7、VC-3.14）：资产交易详情查询服务契约自检。
// 覆盖：详情经 POST 通道带 TransactionId body 透传、Grant 明细列表映射、
// 空 TransactionId 兜底拒绝、区服不归属目标 App 拒绝。
public sealed class OnlineAssetTransactionQueryServiceTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"asset_detail_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _scopeOwner;
    private readonly SqlSugarScopeProvider _db;

    public OnlineAssetTransactionQueryServiceTests()
    {
        var config = new ConnectionConfig
        {
            ConfigId = "online-asset-detail",
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _scopeOwner = new SqlSugarScope(new List<ConnectionConfig> { config });
        _db = _scopeOwner.GetConnectionScope("online-asset-detail");
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(

            typeof(TenantGameAreaEntity));
    }

    /// <summary>
    /// 详情查询：Admin 端点 GET 经 scope 校验后，Online 调用走 POST 通道带 TransactionId body；
    /// 响应（含 Grant 明细列表）原样映射到 Admin 响应（无脱敏面）。
    /// </summary>
    [Fact]
    public async Task DetailAsync_PassesTransactionIdOverPostChannel_AndMapsResponse()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        fake.SetPayload(new OnlineAssetTransactionDetailResponse
        {
            TransactionId = "tx-9527",
            IdempotencyKey = "GFX-GRANT-20260913120000-abcdef01",
            Status = "Succeeded",
            Source = "Admin",
            Reason = "manual grant",
            BusinessOrderNumber = "BO-9527",
            OperatorId = "op-1",
            PlayerId = "player-1",
            OccurredAt = 1780000000,
            GrantItems = new List<OnlineAssetTransactionGrantItemResponse>
            {
                new() { ItemOrCurrencyType = "gold", Quantity = 50, BeforeValue = 100, AfterValue = 150, },
            },
        });
        var service = CreateService(fake);

        var detail = await service.DetailAsync(new OnlineAssetTransactionDetailRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            TransactionId = "tx-9527",
        });

        var call = Assert.Single(fake.Calls);
        Assert.Equal("query_transaction_detail", call.Action);
        Assert.Equal(OnlineAssetTestSupport.Area1001, call.ServerId);
        var body = Assert.IsType<OnlineAssetTransactionDetailQueryRequest>(call.Request);
        Assert.Equal("tx-9527", body.TransactionId);

        Assert.Equal("tx-9527", detail.TransactionId);
        Assert.Equal("Succeeded", detail.Status);
        Assert.Equal("BO-9527", detail.BusinessOrderNumber);
        var grant = Assert.Single(detail.GrantItems);
        Assert.Equal("gold", grant.ItemOrCurrencyType);
        Assert.Equal(50, grant.Quantity);
        Assert.Equal(150, grant.AfterValue);
    }

    /// <summary>
    /// 空 TransactionId 兜底拒绝（ParameterValidationFailed）——DTO [Required] 不覆盖 GET 复杂查询对象路径。
    /// </summary>
    [Fact]
    public async Task DetailAsync_RejectsEmptyTransactionId()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        var service = CreateService(fake);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.DetailAsync(new OnlineAssetTransactionDetailRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            TransactionId = string.Empty,
        }));

        Assert.Equal((int)CommonErrorCode.ParameterValidationFailed, exception.Code);
        Assert.Empty(fake.Calls);
    }

    /// <summary>
    /// 区服不存在时拒绝，Online 调用零次。
    /// </summary>
    [Fact]
    public async Task DetailAsync_RejectsNonexistentServer()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        var service = CreateService(fake);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.DetailAsync(new OnlineAssetTransactionDetailRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = 9999,
            TransactionId = "tx-1",
        }));

        Assert.Equal((int)OnlineErrorCode.ScopeDenied, exception.Code);
        Assert.Empty(fake.Calls);
    }

    private OnlineAssetTransactionQueryService CreateService(IOnlineServerClient onlineServerClient)
    {
        var factory = new OnlineScopeContextFactory(OnlineAssetTestSupport.Repository<TenantGameAreaEntity>(_db));
        return new OnlineAssetTransactionQueryService(onlineServerClient, factory);
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
