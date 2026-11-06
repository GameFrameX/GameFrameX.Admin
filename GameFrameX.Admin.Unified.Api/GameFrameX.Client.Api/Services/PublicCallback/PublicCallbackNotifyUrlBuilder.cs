using System.Globalization;

namespace GameFrameX.Client.Api.Services.PublicCallback;

/// <summary>
/// 公开回调通知地址构造器。
/// <para>管理员配置可能是：站点根地址（含反向代理前缀）或完整的新回调路由地址。
/// 本构造器统一规范化为能命中当前公开回调端点（<c>{packageId}</c> 可选路由模板）的完整地址。
/// 分包段以当前下单上下文的分包标识为准（无则生成不带分包段的主库地址）。
/// 无法识别的路径返回 null，由调用方在下单前拒绝，避免网关回调打在无法命中的地址上导致已付款订单不发货。</para>
/// </summary>
public static class PublicCallbackNotifyUrlBuilder
{
    /// <summary>
    /// 构造能命中公开回调端点的完整通知地址。
    /// </summary>
    /// <param name="configuredNotifyUrl">管理员配置的通知地址（站点根地址 / 完整新地址）。</param>
    /// <param name="callbackRoute">回调端点路由段（如 api/GameOrder/NotifyAliPay，不含分包段）。</param>
    /// <param name="packageId">当前下单上下文的分包标识；无值时不生成分包段。</param>
    /// <returns>完整回调地址；配置为空或无法识别时返回 null。</returns>
    public static string? Build(string? configuredNotifyUrl, string callbackRoute, long? packageId)
    {
        if (string.IsNullOrWhiteSpace(configuredNotifyUrl) || string.IsNullOrWhiteSpace(callbackRoute))
        {
            return null;
        }

        if (!Uri.TryCreate(configuredNotifyUrl.Trim(), UriKind.Absolute, out var configuredUri)
            || (configuredUri.Scheme != Uri.UriSchemeHttp && configuredUri.Scheme != Uri.UriSchemeHttps))
        {
            return null;
        }

        var pathSegments = SplitPath(configuredUri.AbsolutePath);
        var routeSegments = SplitPath(callbackRoute);
        var sitePrefixSegments = ExtractSitePrefixSegments(pathSegments, routeSegments);
        if (sitePrefixSegments == null)
        {
            return null;
        }

        var prefixPath = sitePrefixSegments.Count == 0 ? string.Empty : "/" + string.Join("/", sitePrefixSegments);
        var packageSegment = packageId is > 0 ? "/" + packageId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
        var uriBuilder = new UriBuilder(configuredUri.Scheme, configuredUri.Host, configuredUri.Port)
        {
            Path = prefixPath + "/" + callbackRoute.Trim('/') + packageSegment,
        };
        return uriBuilder.Uri.ToString();
    }

    /// <summary>
    /// 提取回调路由之前的站点前缀段：空路径（站点根）返回空列表；
    /// 路径必须以回调路由段结尾，或以回调路由段加单一 packageId 段结尾，
    /// 否则视为无法识别的地址返回 null。
    /// </summary>
    /// <param name="pathSegments">配置地址的路径段。</param>
    /// <param name="routeSegments">回调端点路由段。</param>
    /// <returns>站点前缀段；无法识别时返回 null。</returns>
    private static List<string>? ExtractSitePrefixSegments(List<string> pathSegments, List<string> routeSegments)
    {
        if (pathSegments.Count == 0)
        {
            return new List<string>();
        }

        var effectiveSegments = pathSegments;
        if (pathSegments.Count > routeSegments.Count
            && pathSegments[^1].All(char.IsDigit)
            && EndsWithSegments(pathSegments.Take(pathSegments.Count - 1).ToList(), routeSegments))
        {
            effectiveSegments = pathSegments.Take(pathSegments.Count - 1).ToList();
        }

        if (effectiveSegments.Count < routeSegments.Count || !EndsWithSegments(effectiveSegments, routeSegments))
        {
            return null;
        }

        return effectiveSegments.Take(effectiveSegments.Count - routeSegments.Count).ToList();
    }

    /// <summary>
    /// 判断段列表是否以指定路由段结尾（忽略大小写按段比较）。
    /// </summary>
    /// <param name="segments">路径段。</param>
    /// <param name="tailSegments">路由段。</param>
    /// <returns>是否以路由段结尾。</returns>
    private static bool EndsWithSegments(List<string> segments, List<string> tailSegments)
    {
        for (var index = 1; index <= tailSegments.Count; index++)
        {
            if (!string.Equals(segments[segments.Count - index], tailSegments[tailSegments.Count - index], StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 按斜杠拆分路径为非空段列表。
    /// </summary>
    /// <param name="path">路径或路由段字符串。</param>
    /// <returns>非空路径段列表。</returns>
    private static List<string> SplitPath(string path)
    {
        return path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }
}
