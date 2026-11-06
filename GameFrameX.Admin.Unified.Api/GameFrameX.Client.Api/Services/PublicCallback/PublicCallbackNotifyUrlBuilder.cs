using System.Globalization;

namespace GameFrameX.Client.Api.Services.PublicCallback;

/// <summary>
/// 公开回调通知地址构造器。
/// <para>管理员配置可能是：站点根地址（含反向代理前缀）、完整的新回调路由地址、或路由升级前遗留的
/// 旧「回调/{id}/{appId}/{packageId}」三段地址。本构造器统一规范化为能命中当前公开回调端点
/// （<c>{packageId}</c> 可选路由模板）的完整地址：路由段之后遗留的纯数字尾段（至多 3 段）一律剥除后
/// 按当前上下文重建，分包段以当前下单上下文的分包标识为准（无则生成不带分包段的主库地址）。
/// 无法识别的路径返回 null，由调用方在下单前拒绝，避免网关回调打在无法命中的地址上导致已付款订单不发货。</para>
/// </summary>
public static class PublicCallbackNotifyUrlBuilder
{
    /// <summary>
    /// 路由升级前回调地址末尾携带的最大纯数字段数（租户 / 应用 / 分包三段）。
    /// </summary>
    private const int LegacyRouteTailMaximum = 3;

    /// <summary>
    /// 构造能命中公开回调端点的完整通知地址。
    /// </summary>
    /// <param name="configuredNotifyUrl">管理员配置的通知地址（站点根地址 / 完整新地址 / 旧三段地址）。</param>
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
    /// 路径必须以回调路由段结尾（允许其后至多 3 段纯数字遗留尾段，剥除后不参与结果），
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
        var numericTailCount = CountTrailingNumericSegments(pathSegments);
        if (numericTailCount > 0)
        {
            if (numericTailCount > LegacyRouteTailMaximum)
            {
                return null;
            }

            effectiveSegments = pathSegments.Take(pathSegments.Count - numericTailCount).ToList();
        }

        if (effectiveSegments.Count < routeSegments.Count || !EndsWithSegments(effectiveSegments, routeSegments))
        {
            return null;
        }

        return effectiveSegments.Take(effectiveSegments.Count - routeSegments.Count).ToList();
    }

    /// <summary>
    /// 统计路径末尾连续的纯数字段数量。
    /// </summary>
    /// <param name="segments">路径段。</param>
    /// <returns>末尾连续纯数字段数量。</returns>
    private static int CountTrailingNumericSegments(List<string> segments)
    {
        var count = 0;
        for (var index = segments.Count - 1; index >= 0; index--)
        {
            if (segments[index].Length == 0 || !segments[index].All(char.IsDigit))
            {
                break;
            }

            count++;
        }

        return count;
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
