using GameFrameX.Client.Api.Services.PublicCallback;
using GameFrameX.Client.Api.Integrations.Payment;

namespace GameFrameX.Client.Api.Middleware;

/// <summary>
/// 公开 provider 回调路由解析中间件。
/// <para>该中间件只处理白名单回调入口；provider 签名或 JWT 仍由具体回调 action 使用主库配置校验。</para>
/// </summary>
public sealed class PublicCallbackTenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public PublicCallbackTenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, PublicCallbackRouteResolver routeResolver)
    {
        if (!routeResolver.IsPublicCallback(context))
        {
            await _next(context);
            return;
        }

        var result = routeResolver.Resolve(context);
        if (!result.IsSuccess)
        {
            await MiddlewareResponseWriter.WriteRejectedAsync(context, StatusCodes.Status400BadRequest, result.ErrorMessage ?? string.Empty);
            return;
        }

        var routeContext = result.Context;
        if (routeContext == null)
        {
            await MiddlewareResponseWriter.WriteRejectedAsync(context, StatusCodes.Status400BadRequest, "回调路由解析失败：缺少上下文");
            return;
        }

        context.Items[PublicCallbackRouteContext.ItemKey] = routeContext;
        if (routeContext.PackageId.HasValue)
        {
            context.Items[PaymentOptionSettingProvider.PackageIdKey] = routeContext.PackageId.Value;
        }

        await _next(context);
    }
}
