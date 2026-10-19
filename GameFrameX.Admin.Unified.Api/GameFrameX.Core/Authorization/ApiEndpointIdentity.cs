namespace GameFrameX.Core.Authorization;

/// <summary>
/// API 端点路由归一化（U6/C42）。
/// <para>统一三类输入到同一规范形式 <c>api/&lt;lower&gt;</c>：活端点模板、routeName（GetRouteName 输出）、请求 Path。
/// 规范形式即 api_endpoint.RoutePath，也是端点↔按钮绑定的对账键。<b>RoutePath 不是授权令牌</b>，仅用于对账与请求期查端点身份。</para>
/// </summary>
public static class ApiEndpointIdentity
{
    /// <summary>
    /// 把任意路由串（活端点模板 / 请求 Path / routeName 加 api/ 前缀）归一化为 <c>api/...</c> 小写形式。
    /// </summary>
    public static string NormalizeRoutePath(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var p = raw.Trim().Trim('/');
        if (p.StartsWith("api/", StringComparison.OrdinalIgnoreCase))
        {
            p = p.Substring(4);
        }

        p = p.Trim('/');
        if (string.IsNullOrEmpty(p))
        {
            return string.Empty;
        }

        return "api/" + p.ToLowerInvariant();
    }

    /// <summary>
    /// HTTP Method 归一化为大写形式。
    /// </summary>
    public static string NormalizeHttpMethod(string? httpMethod)
    {
        return string.IsNullOrWhiteSpace(httpMethod)
                   ? string.Empty
                   : httpMethod.Trim().ToUpperInvariant();
    }

    /// <summary>
    /// 构造请求期端点键，必须同时包含 HTTP Method 与 RoutePath，避免同一路径不同方法共用授权。
    /// </summary>
    public static string ToEndpointKey(string? httpMethod, string? routePath)
    {
        var method = NormalizeHttpMethod(httpMethod);
        var path = NormalizeRoutePath(routePath);
        return string.IsNullOrEmpty(method) || string.IsNullOrEmpty(path)
                   ? string.Empty
                   : method + " " + path;
    }

    /// <summary>
    /// routeName（GetRouteName 输出，如 <c>systemUser:page</c>）→ 规范 RoutePath（<c>api/systemuser/page</c>）。
    /// 一次性迁移用：旧 .g.cs Permission 字段 / DB global_menu.Permission 均为 routeName 形态。
    /// </summary>
    public static string RouteNameToRoutePath(string? routeName)
    {
        if (string.IsNullOrWhiteSpace(routeName))
        {
            return string.Empty;
        }

        var asPath = routeName.Replace(":", "/");
        return NormalizeRoutePath("api/" + asPath);
    }

    /// <summary>
    /// 请求期从 Request.Path 规范化为 RoutePath。
    /// </summary>
    public static string FromRequestPath(string? requestPath)
    {
        return NormalizeRoutePath(requestPath);
    }

    /// <summary>
    /// 请求期从 Request.Method + Request.Path 构造端点键。
    /// </summary>
    public static string FromRequestEndpointKey(string? httpMethod, string? requestPath)
    {
        return ToEndpointKey(httpMethod, requestPath);
    }

    /// <summary>
    /// 请求期从 Request.Method + 路由模板构造端点键；模板缺失时退回 Request.Path。
    /// </summary>
    public static string FromRequestEndpointKey(string? httpMethod, string? requestPath, string? endpointRouteTemplate)
    {
        var routePath = string.IsNullOrWhiteSpace(endpointRouteTemplate)
                            ? FromRequestPath(requestPath)
                            : NormalizeRoutePath(endpointRouteTemplate);
        return ToEndpointKey(httpMethod, routePath);
    }
}