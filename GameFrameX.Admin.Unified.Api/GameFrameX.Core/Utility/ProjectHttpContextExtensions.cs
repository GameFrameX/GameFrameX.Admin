namespace GameFrameX.Core.Utility;

/// <summary>
/// HTTP 上下文兼容扩展。
/// </summary>
public static class ProjectHttpContextExtensions
{
    /// <summary>
    /// 将新旧令牌写入响应头。
    /// </summary>
    public static void SetTokensOfResponseHeaders(this HttpContext httpContext, string accessToken, string refreshToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        httpContext.Response.Headers["access-token"] = accessToken;
        httpContext.Response.Headers["x-access-token"] = refreshToken;
    }

    /// <summary>
    /// 获取远端 IPv4 地址。
    /// </summary>
    public static string GetRemoteIpAddressToIPv4(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        var address = httpContext.Connection.RemoteIpAddress;
        if (address == null)
        {
            return string.Empty;
        }

        return address.IsIPv4MappedToIPv6 ? address.MapToIPv4().ToString() : address.ToString();
    }
}