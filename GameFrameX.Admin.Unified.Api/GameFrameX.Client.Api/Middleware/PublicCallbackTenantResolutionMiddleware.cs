using GameFrameX.Client.Api.Services.PublicCallback;
using GameFrameX.Client.Api.Services;
using GameFrameX.Client.Api.Integrations.Payment;
using GameFrameX.Core.Const;
using GameFrameX.Core.Service.Tenant;

namespace GameFrameX.Client.Api.Middleware;

/// <summary>
/// 公开 provider 回调租户解析中间件。
/// <para>该中间件只处理白名单回调入口，并在 MVC 控制器构造前建立租户上下文；provider 签名或 JWT 仍由具体回调 action 使用租户配置校验。</para>
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

        context.Items[TenantContextAccessor.TenantIdKey] = routeContext.TenantId;
        context.Items[ClaimConst.TenantId] = routeContext.TenantId;
        context.Items[AppResolutionMiddleware.AppIdKey] = routeContext.AppId;
        context.Items[PublicCallbackRouteContext.ItemKey] = routeContext;
        if (routeContext.PackageId.HasValue)
        {
            context.Items[PaymentOptionSettingProvider.PackageIdKey] = routeContext.PackageId.Value;
        }

        using (TenantOperationContext.UseTenant(routeContext.TenantId))
        {
            await _next(context);
        }
    }
}