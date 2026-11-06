using GameFrameX.Client.Api.GameAnnouncement;
using GameFrameX.Client.Api.GameUser;
using GameFrameX.Client.Api.Integrations.Advertisement;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace GameFrameX.Core.Tests.ClientApi;

public sealed class ClientApiEndpointCoverageTests
{
    [Fact]
    public void Matrix_CoversEveryClientApiControllerAction()
    {
        var actual = DiscoverClientApiActions();
        var expected = ClientApiEndpointCoverageMatrix.ByKey.Keys.Order(StringComparer.Ordinal).ToArray();

        Assert.Equal(expected, actual.Keys.Order(StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void Matrix_DeclaresSuccessAndFailureBoundaryForEveryAction()
    {
        foreach (var endpoint in ClientApiEndpointCoverageMatrix.ByKey.Values)
        {
            Assert.False(string.IsNullOrWhiteSpace(endpoint.SuccessCase));
            Assert.NotEmpty(endpoint.FailureCases);
            Assert.All(endpoint.FailureCases, failure => Assert.False(string.IsNullOrWhiteSpace(failure)));
        }
    }

    [Fact]
    public void Matrix_RouteAndHttpMethodMatchControllerAttributes()
    {
        var actual = DiscoverClientApiActions();

        foreach (var endpoint in ClientApiEndpointCoverageMatrix.ByKey.Values)
        {
            var action = actual[endpoint.Key];

            Assert.Equal(endpoint.HttpMethod, action.HttpMethod);
            Assert.Equal(endpoint.Route, action.Route);
        }
    }

    [Fact]
    public void PublicCallbackEndpoints_DeclareOptionalPackageBoundary()
    {
        var callbacks = ClientApiEndpointCoverageMatrix.ByKey.Values
                                                       .Where(endpoint => endpoint.IsPublicCallback)
                                                       .Where(endpoint => endpoint.Group == "payment-callback" || endpoint.Group == "web-shop-callback")
                                                       .ToArray();

        Assert.NotEmpty(callbacks);
        foreach (var endpoint in callbacks)
        {
            Assert.Contains("invalid-route", endpoint.FailureCases);
            Assert.DoesNotContain("{tenantId:long}", endpoint.Route); // c406-keep
            Assert.DoesNotContain("{appId:long}", endpoint.Route); // c406-keep
            Assert.False(endpoint.Requires);
            Assert.False(endpoint.RequiresApp);
        }
    }

    [Fact]
    public void AccountLoginEndpoints_ProtectPascalCaseAccessTokenContract()
    {
        var loginEndpoints = ClientApiEndpointCoverageMatrix.ByKey.Values
                                                            .Where(endpoint => endpoint.SuccessCase.Contains("returns AccessToken", StringComparison.Ordinal))
                                                            .ToArray();

        Assert.NotEmpty(loginEndpoints);
        Assert.All(loginEndpoints, endpoint => Assert.Contains("pascal-case", endpoint.FailureCases));
    }

    [Fact]
    public async Task AdvertisementCallback_LegacyThreeSegmentRoute_ReturnsNotFound()
    {
        using var host = await new HostBuilder()
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
                         .StartAsync();

        using var client = host.GetTestClient();
        var response = await client.GetAsync("/api/GameAdWebHook/TopOnNotify/100/200/300"); // c406-keep

        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    private static IReadOnlyDictionary<string, DiscoveredEndpoint> DiscoverClientApiActions()
    {
        var controllerTypes = typeof(GameUserController).Assembly
                                                        .GetTypes()
                                                        .Where(type => type.IsClass &&
                                                                       !type.IsAbstract &&
                                                                       type.Namespace != null && type.Namespace.StartsWith("GameFrameX.Client.Api", StringComparison.Ordinal) &&
                                                                       typeof(Controller).IsAssignableFrom(type))
                                                        .OrderBy(type => type.FullName, StringComparer.Ordinal);

        var endpoints = new Dictionary<string, DiscoveredEndpoint>(StringComparer.Ordinal);
        foreach (var controllerType in controllerTypes)
        {
            var controllerName = TrimControllerSuffix(controllerType.Name);
            foreach (var method in controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                var httpAttribute = method.GetCustomAttributes<HttpMethodAttribute>(false).SingleOrDefault();
                if (httpAttribute == null)
                {
                    continue;
                }

                var httpMethod = httpAttribute.HttpMethods.Single().ToUpperInvariant();
                var actionRoute = httpAttribute.Template;
                // C300（GFX-708）：controller 自身显式声明 [Route] 时路由前缀以声明为准（如改名后锁定旧路径的
                // GameAnnouncementController）；未声明的沿用基类 token 惯例 api/{controllerName}
                var declaredRoute = controllerType.GetCustomAttributes<RouteAttribute>(false).SingleOrDefault();
                var routePrefix = declaredRoute == null
                                      ? $"api/{controllerName}"
                                      : declaredRoute.Template.Replace("[controller]", controllerName);
                var route = routePrefix.Contains("[action]", StringComparison.Ordinal)
                                ? routePrefix.Replace("[action]", method.Name)
                                : $"{routePrefix}/{method.Name}";
                if (!string.IsNullOrWhiteSpace(actionRoute))
                {
                    route = $"{route}/{actionRoute}";
                }

                var key = $"{controllerName}.{method.Name}";
                endpoints.Add(key, new DiscoveredEndpoint(key, httpMethod, route));
            }
        }

        return endpoints;
    }

    private static string TrimControllerSuffix(string typeName)
    {
        const string suffix = "Controller";
        return typeName.EndsWith(suffix, StringComparison.Ordinal)
                   ? typeName[..^suffix.Length]
                   : typeName;
    }

    private sealed record DiscoveredEndpoint(string Key, string HttpMethod, string Route);
}
