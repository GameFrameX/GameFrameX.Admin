using GameFrameX.Core.Middleware;
using GameFrameX.Core.SqlSugar;
using Microsoft.AspNetCore.Http;

namespace GameFrameX.Core.Tests.ClientApi;

public class TenantResolutionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WithTenantIdHeader_EstablishesTenantContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[GlobalConst.TenantIdHeader] = "123";
        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal(123L, context.Items[TenantContextAccessor.TenantIdKey]);
    }

    [Fact]
    public async Task InvokeAsync_WithLegacyApiKeyHeader_DoesNotEstablishTenantContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[GlobalConst.ApiKeyHeader] = "legacy-api-key";
        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.False(context.Items.ContainsKey(TenantContextAccessor.TenantIdKey));
    }

    [Theory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("not-a-number")]
    public async Task InvokeAsync_WithInvalidTenantIdHeader_DoesNotEstablishTenantContext(string tenantId)
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[GlobalConst.TenantIdHeader] = tenantId;
        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.False(context.Items.ContainsKey(TenantContextAccessor.TenantIdKey));
    }

    [Fact]
    public async Task InvokeAsync_WithTenantSecretAndSignatureHeaders_StoresRawContextValues()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[GlobalConst.TenantSecretHeader] = "tenant-secret";
        context.Request.Headers[GlobalConst.SignatureHeader] = "request-signature";
        var middleware = new TenantResolutionMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal("tenant-secret", context.Items[TenantContextAccessor.TenantSecretKey]);
        Assert.Equal("request-signature", context.Items[TenantContextAccessor.SignatureKey]);
        Assert.False(context.Items.ContainsKey(TenantContextAccessor.TenantIdKey));
    }
}