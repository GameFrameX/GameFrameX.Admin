using System.Globalization;
using GameFrameX.Core.Service.Tenant;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace GameFrameX.Client.Api.Middleware;

/// <summary>
/// 租户访问门禁中间件（Client.Api 端，注册于 <c>Program.cs</c>，<c>UseRouting</c> 之后）。
/// <para>对携带租户上下文的请求，按租户启用状态与到期状态裁决：</para>
/// <list type="bullet">
/// <item>租户禁用（<c>IsEnabled==false</c>）→ 403 TenantErrorCode.TenantDisabled（禁读禁写，「禁用即无法使用」）。</item>
/// <item>租户过期（过渡期结束）且写操作 → 403 TenantErrorCode.TenantExpired；读操作放行。</item>
/// <item>过渡期 / 正常 → 放行。</item>
/// </list>
/// <para>放行：公开回调（支付 notify 等，由 <c>PublicCallbackTenantResolutionMiddleware</c> 标记 <c>PublicCallbackRouteContext.ItemKey</c>；回调即使租户过期/禁用也须处理，避免丢单）、无租户上下文请求。</para>
/// <para>写操作按 MVC action 方法名前缀判定，见 <see cref="TenantReadActionClassifier"/>；<c>UseRouting</c> 之后可通过 <c>HttpContext.GetEndpoint</c> 取 action 方法名。</para>
/// </summary>
public class TenantExpirationMiddleware
{
    private readonly RequestDelegate _next;

    public TenantExpirationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, TenantAccessGate tenantAccessGate)
    {
        // 公开回调（支付 notify 等）放行：无登录态，且租户过期/禁用时回调仍须处理（用户可能已付款）。
        if (context.Items.ContainsKey(PublicCallbackRouteContext.ItemKey))
        {
            await _next(context);
            return;
        }

        var tenantIdObj = context.Items[TenantContextAccessor.TenantIdKey];
        if (tenantIdObj == null)
        {
            await _next(context);
            return;
        }

        var tenantId = Convert.ToInt64(tenantIdObj);
        var isWriteAction = !TenantReadActionClassifier.IsReadAction(ResolveActionName(context));
        var verdict = await tenantAccessGate.EvaluateAsync(tenantId, isWriteAction);

        if (verdict == TenantAccessVerdict.Allow)
        {
            await _next(context);
            return;
        }

        var errorCode = verdict == TenantAccessVerdict.DenyExpiredWrite
                            ? TenantErrorCode.TenantExpired
                            : TenantErrorCode.TenantDisabled;
        var message = verdict == TenantAccessVerdict.DenyExpiredWrite ? "租户已过期" : "租户已禁用";

        await MiddlewareResponseWriter.WriteRejectedAsync(
            context,
            StatusCodes.Status403Forbidden,
            message,
            Convert.ToInt32(errorCode, CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// 解析当前 endpoint 的 MVC action 方法名（<c>UseRouting</c> 之后 endpoint 已匹配）。
    /// </summary>
    private static string? ResolveActionName(HttpContext context)
    {
        var actionDescriptor = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
        return actionDescriptor?.MethodInfo?.Name;
    }
}