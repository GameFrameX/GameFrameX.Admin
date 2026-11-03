using GameFrameX.Entity.Tenant.Game.App;
using SqlSugar;

namespace GameFrameX.Client.Api.Services.PublicCallback;

/// <summary>
/// 公开 provider 回调 URL 路由参数解析与校验服务。
/// <para>旧 URL 的租户与应用段仅保留参数绑定；provider 与回调类型由 action 确定，配置始终读取主库。</para>
/// </summary>
public sealed class PublicCallbackRouteResolver
{
    private readonly ISqlSugarClient _db;
    public PublicCallbackRouteResolver(ISqlSugarClient db)
    {
        _db = db;
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

        if (!TryReadPositiveLong(context, "tenantId", out _))
        {
            return PublicCallbackRouteResolveResponse.Fail("缺少或非法的 tenantId");
        }

        if (!TryReadPositiveLong(context, "appId", out _))
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

        // 旧 URL 段不再选择数据库或安全配置。
        if (packageId.HasValue && !PackageMatches(packageId.Value))
        {
            return PublicCallbackRouteResolveResponse.Fail("分包不存在或未启用");
        }

        return PublicCallbackRouteResolveResponse.Success(new PublicCallbackRouteContext(
                                                            endpoint.Provider,
                                                            endpoint.CallbackKind,
                                                            packageId));
    }

    private bool PackageMatches(long packageId)
    {
        return _db.Queryable<TenantGameAppPackageEntity>()
                  .Any(package => package.Id == packageId && package.IsEnabled == true);
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