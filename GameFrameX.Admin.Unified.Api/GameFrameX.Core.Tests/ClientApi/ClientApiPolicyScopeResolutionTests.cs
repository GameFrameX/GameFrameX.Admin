using GameFrameX.Client.Api.Middleware;
using GameFrameX.Client.Api.Services.ClientApiTenantOptions;
using GameFrameX.Core.Middleware;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Tenant.ClientApiConfig;
using Microsoft.AspNetCore.Http;

namespace GameFrameX.Core.Tests.ClientApi;

// C378（GFX-838）：策略合并表 (TenantId, AppId) 作用域解析矩阵。
// change：gfx-doc/dev/game-business/changes/C378-client-api-policy-merge/change.md
// 令牌节仅消费租户默认行（AppId=0，维持原租户级语义）；票据节/输出节应用级优先、
// AppId=0 兜底；无行时回落平台默认值。同一 Scoped 实例请求级缓存（重复调用不重复查库）。
public sealed class ClientApiPolicyScopeResolutionTests : IDisposable
{
    private readonly ClientApiTestDb _db = new();
    private readonly long _now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public ClientApiPolicyScopeResolutionTests()
    {
        _db.InitTenantTables(typeof(TenantClientApiPolicyEntity));
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task TokenSection_AlwaysReadsTenantDefaultRow_IgnoresAppLevelRow()
    {
        _db.TenantDb.Insertable(new[]
        {
            new TenantClientApiPolicyEntity { Id = 1, TenantId = _db.TenantId, AppId = 0, SigningKey = "tenant-default-key", LifetimeMinutes = 60, IsEnabled = true, },
            new TenantClientApiPolicyEntity { Id = 2, TenantId = _db.TenantId, AppId = _db.AppId, SigningKey = "app-level-key", LifetimeMinutes = 999, IsEnabled = true, },
        }).ExecuteCommand();

        var provider = CreateProvider(includeApp: true);

        var policy = await provider.GetTokenPolicyAsync();

        // 令牌节唯一消费源是 AppId=0 行：应用级行的密钥与有效期对令牌节不可见
        Assert.Equal("tenant-default-key", policy.SigningKey);
        Assert.Equal(60, policy.LifetimeMinutes);
    }

    [Fact]
    public async Task TicketSection_PrefersAppLevelRow_AndFallsBackToTenantDefault()
    {
        _db.TenantDb.Insertable(new[]
        {
            new TenantClientApiPolicyEntity { Id = 1, TenantId = _db.TenantId, AppId = 0, TicketExpireTime = _now + 3600, EnableKeyAutoCreate = false, IsEnabled = true, },
            new TenantClientApiPolicyEntity { Id = 2, TenantId = _db.TenantId, AppId = _db.AppId, TicketExpireTime = _now + 7200, EnableKeyAutoCreate = true, IsEnabled = true, },
        }).ExecuteCommand();

        var provider = CreateProvider(includeApp: true);

        // 应用级优先
        var appPolicy = await provider.GetGameLoginTicketPolicyAsync();
        Assert.Equal(_now + 7200, appPolicy.TicketExpireTime);
        Assert.True(appPolicy.EnableKeyAutoCreate);

        // 无应用级行（另一 AppId）→ 回退租户默认
        var fallbackProvider = CreateProvider(customAppId: _db.AppId + 1);
        var fallbackPolicy = await fallbackProvider.GetGameLoginTicketPolicyAsync();
        Assert.Equal(_now + 3600, fallbackPolicy.TicketExpireTime);
        Assert.False(fallbackPolicy.EnableKeyAutoCreate);
    }

    [Fact]
    public async Task NoRows_FallsBackToPlatformDefaults()
    {
        var provider = CreateProvider(includeApp: true);

        var token = await provider.GetTokenPolicyAsync();
        Assert.Null(token.SigningKey);
        Assert.Equal(ClientApiPlatformDefaults.ClientApiTokenLifetimeMinutesDefault, token.LifetimeMinutes);

        var order = await provider.GetOrderPolicyAsync();
        Assert.False(order.EnableSandBoxRecharge);

        var area = await provider.GetAreaPolicyAsync();
        Assert.False(area.EnableReturnHttpManager);
    }

    [Fact]
    public async Task SameScopedInstance_RepeatedCalls_UseRequestLevelCache()
    {
        _db.TenantDb.Insertable(new TenantClientApiPolicyEntity
        {
            Id = 1,
            TenantId = _db.TenantId,
            AppId = 0,
            SigningKey = "cached-key",
            LifetimeMinutes = 60,
            IsEnabled = true,
        }).ExecuteCommand();

        var provider = CreateProvider(includeApp: false);

        var first = await provider.GetTokenPolicyAsync();
        var second = await provider.GetTokenPolicyAsync();

        // 请求级缓存：第二次调用返回同一行值（不重复查库）；单行缓存区分令牌节默认行与生效行两个槽位
        Assert.Equal(first.SigningKey, second.SigningKey);
        Assert.Equal("cached-key", second.SigningKey);

        // 生效行回退查询也走缓存（同一 Scoped 实例）
        var order = await provider.GetOrderPolicyAsync();
        Assert.False(order.EnableSandBoxRecharge);
    }

    private ClientApiTenantOptionsProvider CreateProvider(bool includeApp = true, long? customAppId = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Items[TenantContextAccessor.TenantIdKey] = _db.TenantId;
        if (includeApp || customAppId.HasValue)
        {
            httpContext.Items[AppResolutionMiddleware.AppIdKey] = customAppId ?? _db.AppId;
        }

        var accessor = new HttpContextAccessor { HttpContext = httpContext, };
        return new ClientApiTenantOptionsProvider(_db.TenantDbProvider, accessor);
    }
}
