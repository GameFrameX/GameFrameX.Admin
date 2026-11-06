using System.Text.Json;
using GameFrameX.Client.Api.Integrations.Payment;
using GameFrameX.Client.Api.Middleware;
using GameFrameX.Client.Api.Services.PublicCallback;
using GameFrameX.Core.Middleware;
using GameFrameX.Entity.Business.Game;
using GameFrameX.Payment.AliPay;
using GameFrameX.Payment.Wechat;
using Microsoft.AspNetCore.Http;

namespace GameFrameX.Core.Tests.ClientApi;

public sealed class PublicCallbackSingleMainTests
{
    [Theory]
    [InlineData("alipay")]
    [InlineData("wechat")]
    public void OutboundPayment_BuildsNotifyUrlFromSiteRootThroughProvider(string channel)
    {
        // 出站下单通知地址经 PaymentOptionSettingProvider 真实构造：站点根地址自动补全新回调路由段
        //（旧 tenantId/appId 查询作用域不参与），生成的 notify_url 能命中公开回调端点。 // c406-keep
        using (var database = new ClientApiTestDb())
        {
            database.InitBusinessTables(typeof(GamePaymentConfigEntity));
            var configJson = channel == "alipay"
                ? JsonSerializer.Serialize(new AliPayOptionSetting { AliPayNotifyUrl = "https://example.test", })
                : JsonSerializer.Serialize(new WechatPayOptionSetting { WechatPayNotifyUrl = "https://example.test", });
            database.TenantDb.Insertable(new GamePaymentConfigEntity
            {
                Id = 1900000005101,
                PackageIdNormalized = 0,
                PayChannel = channel,
                IsSandbox = false,
                IsEnabled = true,
                ConfigJson = configJson,
            }).ExecuteCommand();

            var context = new DefaultHttpContext();
            context.Request.QueryString = new QueryString("?tenantId=123&appId=456"); // c406-keep
            var provider = new PaymentOptionSettingProvider(database.DbProvider, new HttpContextAccessor { HttpContext = context });

            var expectedRoute = channel == "alipay" ? "api/GameOrder/NotifyAliPay" : "api/GameOrder/NotifyWeChatPay";
            var notifyUrl = channel == "alipay"
                ? provider.GetOptionSetting<AliPayOptionSetting>("alipay")?.AliPayNotifyUrl
                : provider.GetOptionSetting<WechatPayOptionSetting>("wechat")?.WechatPayNotifyUrl;
            Assert.Equal("https://example.test/" + expectedRoute, notifyUrl);
        }
    }

    [Fact]
    public async Task IncomingCallback_OldRouteSegments_DoNotCreateScopeContext()
    {
        using (var database = new ClientApiTestDb())
        {
            var context = new DefaultHttpContext();
            context.SetEndpoint(new Endpoint(_ => Task.CompletedTask,
                new EndpointMetadataCollection(new PublicCallbackEndpointAttribute("pangle", "ad")), "callback"));
            context.Request.RouteValues["tenantId"] = 999; // c406-keep
            context.Request.RouteValues["appId"] = 888; // c406-keep
            var invoked = false;
            var middleware = new PublicCallbackResolutionMiddleware(request =>
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
