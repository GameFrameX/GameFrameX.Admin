using GameFrameX.Core.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace GameFrameX.Core.Tests.ClientApi;

/// <summary>
/// ProtoVersionMiddleware 单元测试（change C5 server-envelope-middleware）。
/// 缺头一律 400 拒绝（未上线无旧客户端；IProtoVersionExempt 公开回调入口豁免）；白名单外版本 AuditOnly 告警放行。
/// </summary>
public sealed class ProtoVersionMiddlewareTests
{
    private sealed class InvokeOutcome
    {
        public HttpContext Context { get; init; } = null!;
        public bool NextCalled { get; set; }
    }

    private static DefaultHttpContext CreateContext(string? protoVersion)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        if (protoVersion != null)
        {
            context.Request.Headers[ProtoVersionMiddleware.ProtoVersionHeader] = protoVersion;
        }
        return context;
    }

    private static async Task<InvokeOutcome> InvokeAsync(DefaultHttpContext context, ProtoVersionOptions options)
    {
        var outcome = new InvokeOutcome { Context = context };
        var middleware = new ProtoVersionMiddleware(_ =>
        {
            outcome.NextCalled = true;
            return Task.CompletedTask;
        });
        await middleware.InvokeAsync(context, Options.Create(options), NullLogger<ProtoVersionMiddleware>.Instance);
        return outcome;
    }

    [Fact]
    public async Task KnownVersion_Passes()
    {
        var outcome = await InvokeAsync(CreateContext("cola/1.0.0"), new ProtoVersionOptions());

        Assert.True(outcome.NextCalled);
    }

    [Fact]
    public async Task UnknownVersion_WarnsButStillPasses()
    {
        // AuditOnly：白名单外版本只记 Warning 不拦截（major 变更时才收紧）
        var options = new ProtoVersionOptions();
        var first = await InvokeAsync(CreateContext("cola/2.0.0"), options);
        var second = await InvokeAsync(CreateContext("unknown/9.9.9"), options);

        Assert.True(first.NextCalled);
        Assert.True(second.NextCalled);
    }

    [Fact]
    public async Task MissingHeader_Rejected()
    {
        // 未上线无旧客户端：缺版本头一律 400 拒绝，不进管线
        var outcome = await InvokeAsync(CreateContext(null), new ProtoVersionOptions());

        Assert.False(outcome.NextCalled);
        Assert.Equal(StatusCodes.Status400BadRequest, outcome.Context.Response.StatusCode);
    }

    [Fact]
    public async Task MissingHeader_PublicCallbackEndpoint_Exempt()
    {
        // 外部平台 S2S 回调（PublicCallbackEndpointAttribute 实现 IProtoVersionExempt）不带 X-Proto-Ver，放行
        var context = CreateContext(null);
        context.SetEndpoint(new Endpoint(_ => Task.CompletedTask,
            new EndpointMetadataCollection(new GameFrameX.Client.Api.Services.PublicCallback.PublicCallbackEndpointAttribute("alipay", "payment")),
            "alipay-payment"));

        var outcome = await InvokeAsync(context, new ProtoVersionOptions());

        Assert.True(outcome.NextCalled);
        Assert.Equal(StatusCodes.Status200OK, outcome.Context.Response.StatusCode);
    }
}
