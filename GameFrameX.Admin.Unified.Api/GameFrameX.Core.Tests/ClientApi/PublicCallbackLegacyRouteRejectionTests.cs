using GameFrameX.Client.Api.Integrations.Advertisement;
using GameFrameX.Client.Api.Middleware;
using GameFrameX.Client.Api.Services.PublicCallback;
using GameFrameX.Core.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GameFrameX.Core.Tests.ClientApi;

public sealed class PublicCallbackLegacyRouteRejectionTests
{
    [Theory]
    [InlineData("/api/GameAdWebHook/TopOnNotify/100/200/300")] // c406-keep
    [InlineData("/api/GameAdWebHook/TopOnNotify")]
    [InlineData("/api/GameAdWebHook/PangleGlobalNotify")]
    [InlineData("/api/GameAdWebHook/AdMobNotify")]
    [InlineData("/api/GameAdWebHook/UnityAdsNotify")]
    public async Task AdvertisementCallback_RouteWithoutPackageIdSegment_ReturnsNotFound(string path)
    {
        using (var host = await new HostBuilder()
                           .ConfigureWebHost(webBuilder =>
                           {
                               webBuilder.UseTestServer();
                               webBuilder.ConfigureServices(services =>
                               {
                                   services.AddRouting();
                                   services.AddControllers().AddApplicationPart(typeof(GameAdWebHookController).Assembly);
                               });
                               webBuilder.Configure(app =>
                               {
                                   app.UseRouting();
                                   app.UseEndpoints(endpoints => endpoints.MapControllers());
                               });
                           })
                           .StartAsync())
        {
            using (var client = host.GetTestClient())
            {
                var response = await client.GetAsync(path);

                Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
            }
        }
    }

    [Fact]
    public async Task Middleware_AdvertisementRouteWithoutPackageId_IsRejected_AndLegacyFieldsIgnored()
    {
        using (var database = new ClientApiTestDb())
        {
            var context = new DefaultHttpContext();
            context.SetEndpoint(new Endpoint(_ => Task.CompletedTask,
                new EndpointMetadataCollection(new PublicCallbackEndpointAttribute("pangle", "ad")), "callback"));
            context.Request.RouteValues["tenantId"] = 999; // c406-keep
            context.Request.RouteValues["appId"] = 888; // c406-keep
            context.Request.RouteValues["gameAppId"] = 777; // c406-keep
            var invoked = false;
            var middleware = new PublicCallbackResolutionMiddleware(request =>
            {
                invoked = true;
                return Task.CompletedTask;
            });

            await middleware.InvokeAsync(context, new PublicCallbackRouteResolver(database.MainDb));

            // 广告回调缺 packageId：解析层 fail-closed（纵深防御），中间件 400 短路，不进入端点；
            // 旧作用域字段不触发任何解析，Items 不含旧键
            Assert.False(invoked);
            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
            Assert.False(context.Items.ContainsKey(PublicCallbackRouteContext.ItemKey));
            Assert.DoesNotContain(context.Items.Keys, key => string.Equals(key?.ToString(), "TenantId", StringComparison.OrdinalIgnoreCase)); // c406-keep
            Assert.DoesNotContain(context.Items.Keys, key => string.Equals(key?.ToString(), "AppId", StringComparison.OrdinalIgnoreCase)); // c406-keep
            Assert.DoesNotContain(context.Items.Keys, key => string.Equals(key?.ToString(), "GameAppId", StringComparison.OrdinalIgnoreCase)); // c406-keep
        }
    }
}
