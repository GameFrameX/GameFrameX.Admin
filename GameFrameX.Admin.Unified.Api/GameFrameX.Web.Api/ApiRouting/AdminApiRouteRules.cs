using System.Reflection;
using GameFrameX.Core.Api;
using Microsoft.AspNetCore.Mvc.Routing;

namespace GameFrameX.Web.Api.ApiRouting;

/// <summary>
/// Admin 动态 API 路由命名约定入口。规则本体已下沉 <see cref="AdminApiRouteNaming"/>（Core，C368 唯一事实源），
/// 本类仅保留路由约定编排职责并委托规则计算——与 <see cref="MenuSeedRouteName"/>（菜单种子 Permission 拼接）共享同一份规则。
/// </summary>
public static class AdminApiRouteRules
{
    public static string GetControllerRouteSegment(Type controllerType)
    {
        return AdminApiRouteNaming.ControllerRouteSegment(controllerType);
    }

    public static string GetActionRouteSegment(MethodInfo method, HttpMethodAttribute? httpMethodAttribute)
    {
        return AdminApiRouteNaming.ActionRouteSegment(method, httpMethodAttribute);
    }

    public static string GetDefaultHttpMethod(MethodInfo method)
    {
        return AdminApiRouteNaming.GetDefaultHttpMethod(method);
    }

    public static string CombineRoute(string controllerSegment, string actionSegment, string? attributeTemplate)
    {
        return AdminApiRouteNaming.CombineRoute(controllerSegment, actionSegment, attributeTemplate);
    }

    public static string? GetApiExplorerGroupName(Type controllerType)
    {
        var attribute = controllerType.GetCustomAttribute<ApiDescriptionSettingsAttribute>(true);
        return string.IsNullOrWhiteSpace(attribute?.GroupName) ? null : attribute.GroupName;
    }
}