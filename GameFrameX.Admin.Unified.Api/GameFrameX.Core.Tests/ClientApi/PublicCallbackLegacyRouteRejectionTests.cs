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
    [Fact]
    public async Task AdvertisementCallback_LegacyThreeSegmentRoute_ReturnsNotFound()
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
                var response = await client.GetAsync("/api/GameAdWebHook/TopOnNotify/100/200/300"); // c406-keep

                Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
            }
        }
    }

    [Fact]
    public async Task Middleware_LegacyContextFieldsAreIgnored()
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
            var middleware = new PublicCallbackTenantResolutionMiddleware(request =>
            {
                invoked = true;
                Assert.Single(request.Items);
                Assert.Equal(new PublicCallbackRouteContext("pangle", "ad", null), request.Items[PublicCallbackRouteContext.ItemKey]);
                Assert.DoesNotContain(request.Items.Keys, key => string.Equals(key?.ToString(), "TenantId", StringComparison.OrdinalIgnoreCase)); // c406-keep
                Assert.DoesNotContain(request.Items.Keys, key => string.Equals(key?.ToString(), "AppId", StringComparison.OrdinalIgnoreCase)); // c406-keep
                Assert.DoesNotContain(request.Items.Keys, key => string.Equals(key?.ToString(), "GameAppId", StringComparison.OrdinalIgnoreCase)); // c406-keep
                return Task.CompletedTask;
            });

            await middleware.InvokeAsync(context, new PublicCallbackRouteResolver(database.MainDb));

            Assert.True(invoked);
        }
    }
}
