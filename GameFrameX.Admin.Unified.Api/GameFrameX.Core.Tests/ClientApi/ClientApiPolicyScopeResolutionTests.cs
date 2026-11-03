using GameFrameX.Client.Api.Services.ClientApiTenantOptions;
using GameFrameX.Entity.Tenant.ClientApiConfig;
using Microsoft.AspNetCore.Http;

namespace GameFrameX.Core.Tests.ClientApi;

public sealed class ClientApiPolicyScopeResolutionTests : IDisposable
{
    private readonly ClientApiTestDb _database = new();

    public ClientApiPolicyScopeResolutionTests()
    {
        _database.InitTenantTables(typeof(TenantClientApiPolicyEntity));
    }

    [Fact]
    public async Task AllSections_UseLatestEnabledRowWithoutRequestContext()
    {
        var expireTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600;
        _database.MainDb.Insertable(new[]
        {
            new TenantClientApiPolicyEntity { Id = 1, SigningKey = "old-key", IsEnabled = true, },
            new TenantClientApiPolicyEntity { Id = 2, SigningKey = "current-key", LifetimeMinutes = 60,
                TicketExpireTime = expireTime, EnableKeyAutoCreate = true, EnableSandBoxRecharge = true,
                EnableReturnHttpManager = true, IsEnabled = true, },
            new TenantClientApiPolicyEntity { Id = 3, SigningKey = "disabled-key", IsEnabled = false, },
        }).ExecuteCommand();
        var provider = CreateProvider();

        Assert.Equal("current-key", (await provider.GetTokenPolicyAsync()).SigningKey);
        Assert.Equal(expireTime, (await provider.GetGameLoginTicketPolicyAsync()).TicketExpireTime);
        Assert.True((await provider.GetOrderPolicyAsync()).EnableSandBoxRecharge);
        Assert.True((await provider.GetAreaPolicyAsync()).EnableReturnHttpManager);
    }

    [Fact]
    public async Task DisabledRows_AreNotConsumed()
    {
        _database.MainDb.Insertable(new TenantClientApiPolicyEntity
        {
            Id = 1, SigningKey = "disabled", IsEnabled = false, EnableSandBoxRecharge = true,
        }).ExecuteCommand();
        var provider = CreateProvider();

        Assert.Null((await provider.GetTokenPolicyAsync()).SigningKey);
        Assert.False((await provider.GetOrderPolicyAsync()).EnableSandBoxRecharge);
    }

    [Fact]
    public async Task NoRows_ReturnsPlatformDefaults()
    {
        var provider = CreateProvider();
        var token = await provider.GetTokenPolicyAsync();
        Assert.Null(token.SigningKey);
        Assert.Equal(ClientApiPlatformDefaults.ClientApiTokenLifetimeMinutesDefault, token.LifetimeMinutes);
        Assert.False((await provider.GetOrderPolicyAsync()).EnableSandBoxRecharge);
        Assert.False((await provider.GetAreaPolicyAsync()).EnableReturnHttpManager);
    }

    [Fact]
    public async Task AllSections_ShareOneRequestSnapshot()
    {
        _database.MainDb.Insertable(new TenantClientApiPolicyEntity
        {
            Id = 1, SigningKey = "cached-key", IsEnabled = true,
        }).ExecuteCommand();
        var provider = CreateProvider();
        Assert.Equal("cached-key", (await provider.GetTokenPolicyAsync()).SigningKey);
        _database.MainDb.Insertable(new TenantClientApiPolicyEntity
        {
            Id = 2, SigningKey = "new-key", IsEnabled = true, EnableSandBoxRecharge = true,
        }).ExecuteCommand();

        Assert.False((await provider.GetOrderPolicyAsync()).EnableSandBoxRecharge);
        Assert.Equal("cached-key", (await provider.GetTokenPolicyAsync()).SigningKey);
        Assert.True((await CreateProvider().GetOrderPolicyAsync()).EnableSandBoxRecharge);
    }

    private ClientApiTenantOptionsProvider CreateProvider()
    {
        return new ClientApiTenantOptionsProvider(_database.MainDb, new HttpContextAccessor());
    }

    public void Dispose()
    {
        _database.Dispose();
    }
}
