using GameFrameX.Client.Api.Middleware;
using GameFrameX.Client.Api.Services.PublicCallback;
using GameFrameX.Core.Middleware;
using GameFrameX.Payment.AliPay;
using GameFrameX.Payment.Wechat;
using Microsoft.AspNetCore.Http;

namespace GameFrameX.Core.Tests.ClientApi;

public sealed class PublicCallbackSingleMainTests
{
    [Theory]
    [InlineData("alipay")]
    [InlineData("wechat")]
    public void OutboundPayment_UsesConfiguredNotifyUrlWithoutTenantScope(string channel)
    {
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?tenantId=123&appId=456");
        var setting = channel == "alipay"
            ? (GameFrameX.Payment.Abstract.IPaymentOptionSetting)new AliPayOptionSetting { AliPayNotifyUrl = "https://example.test/notify", }
            : new WechatPayOptionSetting { WechatPayNotifyUrl = "https://example.test/notify", };

        Assert.Equal("https://example.test/notify", channel == "alipay"
            ? ((AliPayOptionSetting)setting).AliPayNotifyUrl
            : ((WechatPayOptionSetting)setting).WechatPayNotifyUrl);
    }

    [Fact]
    public async Task IncomingCallback_OldRouteSegments_DoNotCreateScopeContext()
    {
        using (var database = new ClientApiTestDb())
        {
            var context = new DefaultHttpContext();
            context.SetEndpoint(new Endpoint(_ => Task.CompletedTask,
                new EndpointMetadataCollection(new PublicCallbackEndpointAttribute("pangle", "ad")), "callback"));
            context.Request.RouteValues["tenantId"] = 999;
            context.Request.RouteValues["appId"] = 888;
            var invoked = false;
            var middleware = new PublicCallbackTenantResolutionMiddleware(request =>
            {
                invoked = true;
                Assert.Single(request.Items);
                Assert.Equal(new PublicCallbackRouteContext("pangle", "ad", null), request.Items[PublicCallbackRouteContext.ItemKey]);
                return Task.CompletedTask;
            });

            await middleware.InvokeAsync(context, new PublicCallbackRouteResolver(database.MainDb));

            Assert.True(invoked);
        }
    }
}
