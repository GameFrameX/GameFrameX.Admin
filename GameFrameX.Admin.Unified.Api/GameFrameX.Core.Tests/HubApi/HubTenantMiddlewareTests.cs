using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using GameFrameX.Core.Const;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;
using GameFrameX.Hub.Api.Middleware;

namespace GameFrameX.Core.Tests.HubApi;

/// <summary>
/// HubTenantMiddleware 拒绝响应契约测试（C270）：Hub 错误响应体从纯文本统一为 JSON PascalCase <c>{Code, Message}</c>。
/// </summary>
public sealed class HubTenantMiddlewareTests
{
    [Fact]
    public async Task SuperAdmin_Authenticated_RejectedWith403JsonBody()
    {
        var context = CreateContext();
        context.User = CreatePrincipal(
            (ClaimConst.AccountType, ((int)AccountTypeEnum.SuperAdmin).ToString()),
            (ClaimConst.TenantId, "100"));
        var nextCalled = false;
        var middleware = new HubTenantMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);
        using (var document = await ParseBodyAsync(context))
        {
            Assert.Equal(StatusCodes.Status403Forbidden, document.RootElement.GetProperty("Code").GetInt32());
            Assert.Equal("超级管理员账号不允许调用 Hub.Api", document.RootElement.GetProperty("Message").GetString());
        }
    }

    [Fact]
    public async Task AuthenticatedWithoutValidTenantClaim_RejectedWith403JsonBody()
    {
        var context = CreateContext();
        context.User = CreatePrincipal((ClaimConst.TenantId, "not-a-number"));
        var middleware = new HubTenantMiddleware(_ => throw new InvalidOperationException("不应进入下游"));

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        using (var document = await ParseBodyAsync(context))
        {
            Assert.Equal(StatusCodes.Status403Forbidden, document.RootElement.GetProperty("Code").GetInt32());
            Assert.Equal("缺少有效的租户标识", document.RootElement.GetProperty("Message").GetString());
        }
    }

    [Fact]
    public async Task AnonymousWebHookWithoutTenantHeader_RejectedWith401JsonBody()
    {
        var context = CreateContext();
        context.Request.Path = "/api/TapdWebHook/notify";
        var middleware = new HubTenantMiddleware(_ => throw new InvalidOperationException("不应进入下游"));

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        using (var document = await ParseBodyAsync(context))
        {
            Assert.Equal(StatusCodes.Status401Unauthorized, document.RootElement.GetProperty("Code").GetInt32());
            Assert.Equal("缺少租户标识", document.RootElement.GetProperty("Message").GetString());
        }
    }

    [Fact]
    public async Task AnonymousNonWebHookWithoutTenantHeader_PassesThrough()
    {
        var context = CreateContext();
        context.Request.Path = "/api/Other";
        var nextCalled = false;
        var middleware = new HubTenantMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    private static DefaultHttpContext CreateContext()
    {
        return new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream(),
            },
        };
    }

    private static ClaimsPrincipal CreatePrincipal(params (string Type, string Value)[] claims)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
                                       claims.Select(c => new Claim(c.Type, c.Value)),
                                       "TestAuthentication"));
    }

    private static async Task<JsonDocument> ParseBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        return await JsonDocument.ParseAsync(context.Response.Body);
    }
}