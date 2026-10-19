using System.Reflection;
using Microsoft.AspNetCore.Mvc.Routing;

namespace GameFrameX.Core.Api;

/// <summary>
/// Admin 动态 API 路由命名规则唯一事实源（自 <c>GameFrameX.Web.Api/ApiRouting/AdminApiRouteRules</c> 下沉，C368）。
/// <para>
/// 规则本体只此一份：<c>AdminApiRouteRules</c>（运行期路由约定）与 <see cref="MenuSeedRouteName"/>
/// （菜单种子 Permission 拼接）均委托本类，两侧不可能漂移——GFX-710 式服务改名后
/// 种子 Permission 停留在旧路由名、按钮孤立、端点靠 permission:auto 挂根兜底 403 的根因即规则双写。
/// </para>
/// </summary>
public static class AdminApiRouteNaming
{
    /// <summary>
    /// 控制器路由段：类名去 <c>Service</c> 后缀后转 lowerCamelCase（如 <c>TenantOrganizationService</c> → <c>tenantOrganization</c>）。
    /// </summary>
    public static string ControllerRouteSegment(Type controllerType)
    {
        var name = controllerType.Name;
        if (name.EndsWith("Service", StringComparison.Ordinal))
        {
            name = name[..^"Service".Length];
        }

        return ToLowerCamelCase(name);
    }

    /// <summary>
    /// 动作路由段：<c>[ApiDescriptionSettings(Name)]</c> 优先，否则方法名；
    /// 无 HttpMethodAttribute 且以 Get 开头（长度 &gt; 3）时去 Get 前缀；最后转 lowerCamelCase。
    /// </summary>
    public static string ActionRouteSegment(MethodInfo method, HttpMethodAttribute? httpMethodAttribute)
    {
        var configuredName = method.GetCustomAttribute<ApiDescriptionSettingsAttribute>(true)?.Name;
        var actionName = string.IsNullOrWhiteSpace(configuredName) ? method.Name : configuredName!;
        if (httpMethodAttribute == null && IsGetAction(actionName))
        {
            actionName = actionName[3..];
        }

        return ToLowerCamelCase(actionName);
    }

    /// <summary>
    /// 默认 HTTP 谓词推断：Get 前缀动作用 GET，否则 POST（与 Admin 路由约定一致）。
    /// </summary>
    public static string GetDefaultHttpMethod(MethodInfo method)
    {
        return IsGetAction(method.Name) ? "GET" : "POST";
    }

    /// <summary>动作名是否 Get 前缀形态（长度 &gt; 3）。</summary>
    public static bool IsGetAction(string actionName)
    {
        return actionName.Length > 3 && actionName.StartsWith("Get", StringComparison.Ordinal);
    }

    /// <summary>首字母转小写（已小写或空串原样返回）。</summary>
    public static string ToLowerCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value) || char.IsLower(value[0]))
        {
            return value;
        }

        return char.ToLowerInvariant(value[0]) + value[1..];
    }

    /// <summary>
    /// 合成完整路由：HttpMethodAttribute 带模板时取 <c>api/{controller}/{template}</c>（模板以 / 开头则原样绝对路径），
    /// 否则 <c>api/{controller}/{action}</c>。
    /// </summary>
    public static string CombineRoute(string controllerSegment, string actionSegment, string? attributeTemplate)
    {
        if (!string.IsNullOrWhiteSpace(attributeTemplate))
        {
            var template = attributeTemplate!.Trim();
            if (template.StartsWith("/", StringComparison.Ordinal))
            {
                return template;
            }

            return $"api/{controllerSegment}/{template}";
        }

        return $"api/{controllerSegment}/{actionSegment}";
    }

    /// <summary>
    /// 无 HTTP 谓词特性 GET 动作的单路由参数名（与 AdminApiControllerConvention.GetRouteParameterName 同判定：
    /// GET + 恰一个可路由标量参数）。约定执行期 BindingInfo 尚未从绑定特性填充，From* 特性不阻断。
    /// </summary>
    public static string? GetRouteParameterName(MethodInfo method)
    {
        var parameters = method.GetParameters();
        if (parameters.Length != 1)
        {
            return null;
        }

        var parameter = parameters[0];
        var targetType = Nullable.GetUnderlyingType(parameter.ParameterType) ?? parameter.ParameterType;
        var isRouteType = targetType == typeof(string) || targetType.IsPrimitive || targetType.IsEnum
                          || targetType == typeof(decimal) || targetType == typeof(Guid);
        return isRouteType ? parameter.Name : null;
    }

    /// <summary>
    /// 全链路由推导：控制器段 + 动作段 + 谓词模板 + 无特性 GET 动作的路由参数后缀 → 完整 <c>api/...</c> 路径。
    /// <see cref="MenuSeedRouteName"/> 与种子对账守护测试共用本入口，保证与运行期路由约定逐段一致。
    /// </summary>
    public static string BuildRoutePath(Type controllerType, MethodInfo method)
    {
        var httpMethodAttribute = method.GetCustomAttribute<HttpMethodAttribute>(true);
        var controllerSegment = ControllerRouteSegment(controllerType);
        var actionSegment = ActionRouteSegment(method, httpMethodAttribute);
        var routePath = CombineRoute(controllerSegment, actionSegment, httpMethodAttribute?.Template);
        if (httpMethodAttribute == null && string.Equals(GetDefaultHttpMethod(method), "GET", StringComparison.OrdinalIgnoreCase))
        {
            var routeParameterName = GetRouteParameterName(method);
            if (!string.IsNullOrEmpty(routeParameterName))
            {
                routePath += "/{" + routeParameterName + "}";
            }
        }

        return routePath;
    }
}
