using Microsoft.AspNetCore.Http;

namespace GameFrameX.Core.Utility;

/// <summary>
/// 通用工具类
/// </summary>
public static class CommonUtility
{
    private static IHttpContextAccessor? HttpContextAccessor;

    public static void Configure(IHttpContextAccessor httpContextAccessor)
    {
        HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <summary>
    /// 获取服务地址
    /// </summary>
    /// <returns></returns>
    public static string GetLocalhost()
    {
        var httpContext = HttpContextAccessor?.HttpContext;
        if (httpContext == null)
        {
            return string.Empty;
        }

        var result = $"{httpContext.Request.Scheme}://{httpContext.Request.Host.Value}";
        // 代理模式：获取真正的本机地址
        // X-Original-Host=原始请求
        // X-Forwarded-Server=从哪里转发过来
        if (httpContext.Request.Headers.ContainsKey("X-Original-Host"))
        {
            result = $"{httpContext.Request.Scheme}://{httpContext.Request.Headers["X-Original-Host"]}";
        }

        return result;
    }
}