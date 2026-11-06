using GameFrameX.Entity.Business.Game.App;
using SqlSugar;

namespace GameFrameX.Client.Api.Services.PublicCallback;

/// <summary>
/// 公开 provider 回调 URL 路由参数解析与校验服务。
/// <para>回调配置始终读取主库；回调 URL 只携带可选分包标识。</para>
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

        long? packageId = null;
        if (HasParameter(context, "packageId"))
        {
            if (!TryReadPositiveLong(context, "packageId", out var parsedPackageId))
            {
                return PublicCallbackRouteResolveResponse.Fail("非法的 packageId");
            }

            packageId = parsedPackageId;
        }

        // 纵深防御（R5）：广告回调路由已改为必填 {packageId:long}，缺段请求在路由层即 404；
        // 此分支保证不变量在解析层同样成立——未来若再出现可选 packageId 的广告端点或测试直调本解析器，仍 fail-closed 拒绝
        if (!packageId.HasValue && string.Equals(endpoint.CallbackKind, "ad", StringComparison.OrdinalIgnoreCase))
        {
            return PublicCallbackRouteResolveResponse.Fail("广告回调必须包含 packageId");
        }

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
        return _db.Queryable<GameAppPackageEntity>()
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
