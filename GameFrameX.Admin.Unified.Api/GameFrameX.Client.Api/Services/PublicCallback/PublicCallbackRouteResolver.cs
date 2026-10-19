using GameFrameX.Entity.Tenant.Game.App;
using SqlSugar;

namespace GameFrameX.Client.Api.Services.PublicCallback;

/// <summary>
/// 公开 provider 回调 URL 路由参数解析与校验服务。
/// <para>URL 中只放租户、应用和可选分包；provider 与回调类型由命中的 action 确定。服务不会把这些参数视为支付成功或广告发放授权。</para>
/// </summary>
public sealed class PublicCallbackRouteResolver
{
    private readonly ISqlSugarClient _db;
    private readonly ClientApiTenantDbProvider _tenantDbProvider;
    private readonly AppCacheService _appCacheService;

    public PublicCallbackRouteResolver(ISqlSugarClient db, ClientApiTenantDbProvider tenantDbProvider, AppCacheService appCacheService)
    {
        _db = db;
        _tenantDbProvider = tenantDbProvider;
        _appCacheService = appCacheService;
    }

    /// <summary>
    /// 判断当前请求是否命中公开 provider 回调入口。
    /// </summary>
    public bool IsPublicCallback(HttpContext context)
    {
        return GetEndpoint(context) != null;
    }

    /// <summary>
    /// 从 URL 解析并校验公开 provider 回调上下文。
    /// </summary>
    public PublicCallbackRouteResolveResponse Resolve(HttpContext context)
    {
        var endpoint = GetEndpoint(context);
        if (endpoint == null)
        {
            return PublicCallbackRouteResolveResponse.NotPublicCallback();
        }

        if (!TryReadPositiveLong(context, "tenantId", out var tenantId))
        {
            return PublicCallbackRouteResolveResponse.Fail("缺少或非法的 tenantId");
        }

        if (!TryReadPositiveLong(context, "appId", out var appId))
        {
            return PublicCallbackRouteResolveResponse.Fail("缺少或非法的 appId");
        }

        long? packageId = null;
        if (HasParameter(context, "packageId"))
        {
            if (!TryReadPositiveLong(context, "packageId", out var parsedPackageId))
            {
                return PublicCallbackRouteResolveResponse.Fail("非法的 packageId");
            }

            packageId = parsedPackageId;
        }

        var tenant = _db.Queryable<SystemTenantEntity>()
                        .Filter(null, true)
                        .First(u => u.Id == tenantId);
        if (tenant == null || tenant.IsEnabled == false || string.IsNullOrWhiteSpace(tenant.Connection))
        {
            return PublicCallbackRouteResolveResponse.Fail("租户不存在、未启用或缺少连接配置");
        }

        context.Items[TenantContextAccessor.TenantIdKey] = tenantId;
        if (!_appCacheService.TryGetApp(appId, out var app) ||
            app.TenantId != tenantId ||
            app.IsEnabled == false)
        {
            context.Items.Remove(TenantContextAccessor.TenantIdKey);
            return PublicCallbackRouteResolveResponse.Fail("应用不存在、未启用或不属于当前租户");
        }

        if (packageId.HasValue && !PackageMatches(tenantId, appId, packageId.Value))
        {
            context.Items.Remove(TenantContextAccessor.TenantIdKey);
            return PublicCallbackRouteResolveResponse.Fail("分包不存在、未启用或不属于当前应用");
        }

        return PublicCallbackRouteResolveResponse.Success(new PublicCallbackRouteContext(
                                                            tenantId,
                                                            appId,
                                                            endpoint.Provider,
                                                            endpoint.CallbackKind,
                                                            packageId));
    }

    /// <summary>
    /// 生成带公开回调路由参数的 URL。
    /// </summary>
    public static string AppendRouteParameters(string url, PublicCallbackRouteContext routeContext)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return url;
        }

        var queryIndex = url.IndexOf('?');
        var baseUrl = queryIndex >= 0 ? url[..queryIndex] : url;
        var query = queryIndex >= 0 ? url[queryIndex..] : string.Empty;
        var routeUrl = $"{baseUrl.TrimEnd('/')}" +
                       $"/{Uri.EscapeDataString(routeContext.TenantId.ToString())}" +
                       $"/{Uri.EscapeDataString(routeContext.AppId.ToString())}";
        if (routeContext.PackageId.HasValue)
        {
            routeUrl += $"/{Uri.EscapeDataString(routeContext.PackageId.Value.ToString())}";
        }

        return routeUrl + query;
    }

    private bool PackageMatches(long tenantId, long appId, long packageId)
    {
        var tenantDb = _tenantDbProvider.GetTenantDb(tenantId);
        return tenantDb.Queryable<TenantGameAppPackageEntity>()
                       .Any(u => u.Id == packageId &&
                                 u.TenantId == tenantId &&
                                 u.GameAppId == appId &&
                                 u.IsEnabled != false);
    }

    private static bool TryReadPositiveLong(HttpContext context, string name, out long value)
    {
        return long.TryParse(ReadParameter(context, name), out value) && value > 0;
    }

    private static bool HasParameter(HttpContext context, string name)
    {
        return context.Request.RouteValues.ContainsKey(name);
    }

    private static string? ReadParameter(HttpContext context, string name)
    {
        return context.Request.RouteValues.TryGetValue(name, out var routeValue) ? routeValue?.ToString() : null;
    }

    private static PublicCallbackEndpointAttribute? GetEndpoint(HttpContext context)
    {
        return context.GetEndpoint()?.Metadata.GetMetadata<PublicCallbackEndpointAttribute>();
    }
}