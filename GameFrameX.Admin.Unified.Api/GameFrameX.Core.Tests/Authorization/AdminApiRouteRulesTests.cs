using GameFrameX.Application.Game.Home.GameHome;
using GameFrameX.Core.Service.Auth;
using GameFrameX.Web.Api.ApiRouting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GameFrameX.Core.Tests.Authorization;

public sealed class AdminApiRouteRulesTests
{
    [Fact]
    public void ServiceName_BecomesLowerCamelControllerRoute()
    {
        Assert.Equal("systemAuth", AdminApiRouteRules.GetControllerRouteSegment(typeof(SystemAuthService)));
        Assert.Equal("tenantGameHomeEntity", AdminApiRouteRules.GetControllerRouteSegment(typeof(TenantGameHomeEntityService)));
    }

    [Fact]
    public void DefaultGetAction_UsesGetAndDropsGetPrefix()
    {
        var method = typeof(SystemAuthService).GetMethod(nameof(SystemAuthService.GetCaptchaAsync))!;

        Assert.Equal("GET", AdminApiRouteRules.GetDefaultHttpMethod(method));
        Assert.Equal("captcha", AdminApiRouteRules.GetActionRouteSegment(method, null));
        Assert.Equal("api/systemAuth/captcha", AdminApiRouteRules.CombineRoute("systemAuth", "captcha", null));
    }

    [Fact]
    public void ExplicitHttpMethod_KeepsConfiguredActionName()
    {
        var method = typeof(TenantGameHomeEntityService).GetMethod(nameof(TenantGameHomeEntityService.GetOnlineAsync))!;
        var httpPost = method.GetCustomAttributes(typeof(HttpPostAttribute), true).OfType<HttpPostAttribute>().Single();

        Assert.Equal("getOnline", AdminApiRouteRules.GetActionRouteSegment(method, httpPost));
        Assert.Equal("api/tenantGameHomeEntity/getOnline", AdminApiRouteRules.CombineRoute("tenantGameHomeEntity", "getOnline", httpPost.Template));
    }

    [Fact]
    public void AbsoluteHttpRoute_StaysOutsideApiPrefix()
    {
        var method = typeof(SystemAuthService).GetMethod(nameof(SystemAuthService.SwaggerSubmitUrlAsync))!;
        var httpPost = method.GetCustomAttributes(typeof(HttpPostAttribute), true).OfType<HttpPostAttribute>().Single();

        Assert.Equal("/swagger/submitUrl", AdminApiRouteRules.CombineRoute("systemAuth", "swaggerSubmitUrl", httpPost.Template));
    }

    [Fact]
    public void OpenApiGroups_AreProjectOwned()
    {
        // Web.Api 自己的 OpenApi 文档收敛为单个 web.api；三个 Api 项目各自独立 Swagger、互不聚合，
        // 故 AdminOpenApiGroups 只描述 Web.Api 自身，不应出现 Hub/Client。
        Assert.Equal("web.api", AdminOpenApiGroups.WebGroup.Name);
        Assert.Equal("swagger/web.api/swagger.json", AdminOpenApiGroups.WebGroup.RouteTemplate);
    }

    [Fact]
    public void EndpointMetadata_UsesAspNetCoreHttpMethodMetadata()
    {
        var methods = AdminApiEndpointMetadata.GetHttpMethods(new object[] { new HttpMethodMetadata(new[] { "GET", }), });

        Assert.Equal(new[] { "GET", }, methods);
    }
}