using GameFrameX.Client.Api.Services;

namespace GameFrameX.Client.Api.Middleware;

/// <summary>
/// 应用验证中间件：从 GameFrameX-App-Id + GameFrameX-App-Secret 请求头验证应用身份
/// </summary>
public class AppResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public const string AppIdKey = "AppId";

    public AppResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppCacheService appCacheService)
    {
        var appIdStr = context.Request.Headers[GlobalConst.AppIdHeader].FirstOrDefault();
        var appSecret = context.Request.Headers[GlobalConst.AppSecretHeader].FirstOrDefault();

        var hasAppId = !string.IsNullOrEmpty(appIdStr);
        var hasAppSecret = !string.IsNullOrEmpty(appSecret);

        // 双头皆缺：非应用鉴权场景（健康检查 / swagger / 公共回调路径路由），放行由下游决定
        if (!hasAppId && !hasAppSecret)
        {
            await _next(context);
            return;
        }

        // 单头缺失：凭证不完整，fail-fast 拒绝（C314），不再静默放行导致下游缺上下文抛裸 500
        if (!hasAppId || !hasAppSecret)
        {
            await MiddlewareResponseWriter.WriteRejectedAsync(context, StatusCodes.Status400BadRequest, "应用凭证不完整");
            return;
        }

        if (!long.TryParse(appIdStr, out var appId))
        {
            await MiddlewareResponseWriter.WriteRejectedAsync(context, StatusCodes.Status401Unauthorized, "无效的应用 ID");
            return;
        }

        if (!appCacheService.ValidateApp(appId, appSecret, out var app))
        {
            await MiddlewareResponseWriter.WriteRejectedAsync(context, StatusCodes.Status401Unauthorized, "应用凭证无效");
            return;
        }

        if (app.IsEnabled != true)
        {
            await MiddlewareResponseWriter.WriteRejectedAsync(context, StatusCodes.Status401Unauthorized, "应用已被禁用");
            return;
        }

        var tenantIdObj = context.Items[TenantContextAccessor.TenantIdKey];
        if (tenantIdObj != null)
        {
            var tenantId = Convert.ToInt64(tenantIdObj);
            if (app.TenantId != tenantId)
            {
                await MiddlewareResponseWriter.WriteRejectedAsync(context, StatusCodes.Status403Forbidden, "应用不属于当前租户");
                return;
            }
        }

        context.Items[AppIdKey] = appId;
        await _next(context);
    }
}