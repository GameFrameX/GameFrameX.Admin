using GameFrameX.Application.Game.Online.Asset;
using GameFrameX.Application.Game.Online.Asset.Dto;
using GameFrameX.Application.Game.ServerManager.Online;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using SqlSugar;

namespace GameFrameX.Core.Tests.Online.Asset;

// C340 / GFX-752（vault:C4 S3.7、VC-3.11/3.13、VC-3.14）：人工发放/撤销命令服务契约自检。
// 覆盖：幂等键策略（caller-supplied vs. 自动 GFX-GRANT/REVOKE-{ts}-{hex8}）、
// 业务入参兜底（空理由/空物品/非法数量/撤销缺 transactionId）、
// Online 6xxx DuplicateRequest → 镜像 32500001 透传、跨租户越界前置拒绝。
public sealed class OnlineAssetGrantCommandServiceTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"asset_grant_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _scopeOwner;
    private readonly SqlSugarScopeProvider _db;

    public OnlineAssetGrantCommandServiceTests()
    {
        var config = new ConnectionConfig
        {
            ConfigId = "online-asset-grant",
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _scopeOwner = new SqlSugarScope(new List<ConnectionConfig> { config });
        _db = _scopeOwner.GetConnectionScope("online-asset-grant");
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(
            typeof(TenantGameAppEntity),
            typeof(TenantGameAreaEntity));
    }

    /// <summary>
    /// 幂等键策略 1：caller-supplied BusinessOrderNumber 原样透传为 idempotencyKey，GrantItems 透传，
    /// action=grant_asset。
    /// </summary>
    [Fact]
    public async Task GrantAsync_UsesBusinessOrderNumberAsIdempotencyKey_WhenProvided()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        fake.SetPayload(new OnlineAssetGrantAckResponse
        {
            TransactionId = "tx-1",
            IdempotencyKey = "BO-CALLER-1",
            Status = "Accepted",
        });
        var service = CreateService(fake);

        var response = await service.GrantAsync(new OnlineAssetGrantRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            PlayerId = "player-1",
            Reason = "manual grant",
            BusinessOrderNumber = "BO-CALLER-1",
            GrantItems = new List<OnlineAssetGrantItemRequest>
            {
                new() { ItemOrCurrencyType = "gold", Quantity = 100, },
            },
        });

        var call = Assert.Single(fake.Calls);
        Assert.Equal("grant_asset", call.Action);
        var body = Assert.IsType<OnlineAssetGrantCommandRequest>(call.Request);
        Assert.Equal("BO-CALLER-1", body.IdempotencyKey);
        Assert.Equal("manual grant", body.Reason);
        Assert.Equal("player-1", body.PlayerId);
        Assert.Equal("gold", body.GrantItems[0].ItemOrCurrencyType);
        Assert.Equal(100, body.GrantItems[0].Quantity);

        Assert.Equal("tx-1", response.TransactionId);
        Assert.Equal("BO-CALLER-1", response.IdempotencyKey);
    }

    /// <summary>
    /// 幂等键策略 2：未提供 BusinessOrderNumber 时按 GFX-GRANT-{yyyyMMddHHmmss}-{8 hex} 自动生成。
    /// </summary>
    [Fact]
    public async Task GrantAsync_GeneratesGFX_GRANT_IdempotencyKey_WhenMissing()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        fake.SetPayload(new OnlineAssetGrantAckResponse { TransactionId = "tx-2", Status = "Accepted", });
        var service = CreateService(fake);

        var response = await service.GrantAsync(new OnlineAssetGrantRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            PlayerId = "player-1",
            Reason = "manual grant",
            GrantItems = new List<OnlineAssetGrantItemRequest>
            {
                new() { ItemOrCurrencyType = "gold", Quantity = 1, },
            },
        });

        var call = Assert.Single(fake.Calls);
        var body = Assert.IsType<OnlineAssetGrantCommandRequest>(call.Request);
        Assert.StartsWith("GFX-GRANT-", body.IdempotencyKey);
        Assert.Matches(@"^GFX-GRANT-\d{14}-[0-9a-f]{8}$", body.IdempotencyKey);
        // 响应 IdempotencyKey 由 Online 回填，mock 不模拟此回填；仅断言透传键形态。
        Assert.NotNull(response);
    }

    /// <summary>
    /// 兜底拒绝：Reason 为空 → ParameterValidationFailed，Online 调用零次。
    /// </summary>
    [Fact]
    public async Task GrantAsync_RejectsEmptyReason()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        var service = CreateService(fake);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.GrantAsync(new OnlineAssetGrantRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            PlayerId = "player-1",
            Reason = string.Empty,
            GrantItems = new List<OnlineAssetGrantItemRequest>
            {
                new() { ItemOrCurrencyType = "gold", Quantity = 1, },
            },
        }));

        Assert.Equal((int)CommonErrorCode.ParameterValidationFailed, exception.Code);
        Assert.Empty(fake.Calls);
    }

    /// <summary>
    /// 兜底拒绝：GrantItems 为空 → ParameterValidationFailed，Online 调用零次。
    /// </summary>
    [Fact]
    public async Task GrantAsync_RejectsEmptyGrantItems()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        var service = CreateService(fake);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.GrantAsync(new OnlineAssetGrantRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            PlayerId = "player-1",
            Reason = "manual",
            GrantItems = new List<OnlineAssetGrantItemRequest>(),
        }));

        Assert.Equal((int)CommonErrorCode.ParameterValidationFailed, exception.Code);
        Assert.Empty(fake.Calls);
    }

    /// <summary>
    /// 兜底拒绝：物品 Quantity <= 0 → ParameterValidationFailed，Online 调用零次。
    /// </summary>
    [Fact]
    public async Task GrantAsync_RejectsNonPositiveQuantity()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        var service = CreateService(fake);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.GrantAsync(new OnlineAssetGrantRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            PlayerId = "player-1",
            Reason = "manual",
            GrantItems = new List<OnlineAssetGrantItemRequest>
            {
                new() { ItemOrCurrencyType = "gold", Quantity = 0, },
            },
        }));

        Assert.Equal((int)CommonErrorCode.ParameterValidationFailed, exception.Code);
        Assert.Empty(fake.Calls);
    }

    /// <summary>
    /// 撤销命令：必须携带 TransactionId，且幂等键同样按 GFX-REVOKE-{ts}-{hex8} 自动生成（caller-supplied 透传）。
    /// </summary>
    [Fact]
    public async Task RevokeAsync_GeneratesGFX_REVOKE_IdempotencyKey_WhenMissing()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        fake.SetPayload(new OnlineAssetGrantAckResponse { TransactionId = "tx-r1", Status = "Accepted", });
        var service = CreateService(fake);

        var response = await service.RevokeAsync(new OnlineAssetRevokeRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            PlayerId = "player-1",
            TransactionId = "tx-original",
            Reason = "rollback",
        });

        var call = Assert.Single(fake.Calls);
        Assert.Equal("revoke_asset", call.Action);
        var body = Assert.IsType<OnlineAssetRevokeCommandRequest>(call.Request);
        Assert.Equal("tx-original", body.TransactionId);
        Assert.StartsWith("GFX-REVOKE-", body.IdempotencyKey);
        Assert.Matches(@"^GFX-REVOKE-\d{14}-[0-9a-f]{8}$", body.IdempotencyKey);
        Assert.NotNull(response);
    }

    /// <summary>
    /// 撤销命令：缺少 TransactionId 兜底拒绝，Online 调用零次。
    /// </summary>
    [Fact]
    public async Task RevokeAsync_RejectsMissingTransactionId()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        var service = CreateService(fake);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.RevokeAsync(new OnlineAssetRevokeRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            PlayerId = "player-1",
            TransactionId = string.Empty,
            Reason = "rollback",
        }));

        Assert.Equal((int)CommonErrorCode.ParameterValidationFailed, exception.Code);
        Assert.Empty(fake.Calls);
    }

    /// <summary>
    /// Online 6xxx DuplicateRequest 镜像码 32500001 透传：fake 直接抛 Mirror code 异常，
    /// Service 必须原样上抛 BusinessException(code=32500001)，不吞错也不重映射。
    /// </summary>
    [Fact]
    public async Task GrantAsync_PropagatesOnlineDuplicateRequestMirrorCode()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        fake.FailOn("grant_asset", new BusinessException((global::System.Enum)OnlineErrorCode.DuplicateRequest, "duplicate request"));
        var service = CreateService(fake);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.GrantAsync(new OnlineAssetGrantRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            PlayerId = "player-1",
            Reason = "manual",
            BusinessOrderNumber = "BO-DUP-1",
            GrantItems = new List<OnlineAssetGrantItemRequest>
            {
                new() { ItemOrCurrencyType = "gold", Quantity = 1, },
            },
        }));

        Assert.Equal((int)OnlineErrorCode.DuplicateRequest, exception.Code);
        Assert.Single(fake.Calls);
    }

    /// <summary>
    /// 越界前置拒绝：AppId 不归属当前租户 → ScopeDenied，Online 调用零次（任何 grant/revoke 调用前先卡）。
    /// </summary>
    [Fact]
    public async Task GrantAsync_RejectsCrossTenant_BeforeAnyOnlineCall()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        var service = CreateService(fake);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.GrantAsync(new OnlineAssetGrantRequest
        {
            AppId = 9999,
            ServerId = OnlineAssetTestSupport.Area1001,
            PlayerId = "player-1",
            Reason = "manual",
            GrantItems = new List<OnlineAssetGrantItemRequest>
            {
                new() { ItemOrCurrencyType = "gold", Quantity = 1, },
            },
        }));

        Assert.Equal((int)OnlineErrorCode.ScopeDenied, exception.Code);
        Assert.Empty(fake.Calls);
    }

    private OnlineAssetGrantCommandService CreateService(IOnlineServerClient onlineServerClient)
    {
        var factory = new OnlineScopeContextFactory(OnlineAssetTestSupport.CreateUserContext(), OnlineAssetTestSupport.Repository<TenantGameAppEntity>(_db), OnlineAssetTestSupport.Repository<TenantGameAreaEntity>(_db));
        return new OnlineAssetGrantCommandService(onlineServerClient, factory);
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
