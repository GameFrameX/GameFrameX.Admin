using GameFrameX.Client.Api.Middleware;
using GameFrameX.Client.Api.Services.ClientApiTenantOptions;

namespace GameFrameX.Client.Api.Services;

/// <summary>
/// HttpContext 上下文扩展，用于在公开配置端点强制租户 + 应用上下文
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// 要求当前请求已解析出租户与应用上下文，否则抛出异常拒绝请求。
    /// <para>缺少租户上下文抛 <see cref="MissingTenantContextException"/>（中间件未写入 TenantId）。</para>
    /// <para>缺少应用上下文抛 <see cref="UnauthorizedAccessException"/>，由 ASP.NET 管道转换为 401。</para>
    /// </summary>
    /// <param name="context">当前 HTTP 上下文</param>
    /// <returns>租户与应用上下文（见 <see cref="TenantAppContext"/>）</returns>
    public static TenantAppContext RequireTenantAppContext(this HttpContext context)
    {
        long tenantId = 0;
        if (context.Items.TryGetValue(TenantContextAccessor.TenantIdKey, out var tenantIdObj) && tenantIdObj != null)
        {
            tenantId = Convert.ToInt64(tenantIdObj);
        }

        if (tenantId == 0)
        {
            throw new MissingTenantContextException("缺少租户上下文");
        }

        long appId = 0;
        if (context.Items.TryGetValue(AppResolutionMiddleware.AppIdKey, out var appIdObj) && appIdObj != null)
        {
            appId = Convert.ToInt64(appIdObj);
        }

        if (appId == 0)
        {
            throw new UnauthorizedAccessException("缺少应用上下文");
        }

        return new TenantAppContext(tenantId, appId);
    }
}