using GameFrameX.Core.Const;
using GameFrameX.Core.Middleware;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Hub.Api.Middleware;

/// <summary>
/// Hub 租户中间件：统一将当前请求的租户 Id 写入 <see cref="Microsoft.AspNetCore.Http.HttpContext.Items"/>
/// （统一键 <see cref="TenantContextAccessor.TenantIdKey"/>，C260 收敛；原专有键 <c>__hub_tenant_id</c> 已移除），
/// 供 <c>ISqlSugarTenantIdResolver</c> 读取并路由租户库。
/// <list type="bullet">
/// <item>已鉴权请求：拒绝超级管理员；从 JWT 解析 <see cref="ClaimConst.TenantId"/>。</item>
/// <item>匿名 WebHook 请求：从 <see cref="WebHookTenantIdHeader"/> 请求头解析租户 Id；WebHook 路由无租户头返回 401。</item>
/// </list>
/// 租户存在性 / 连接配置校验由 Repository 实时查库承担（已废弃 TenantDbResolver 内存缓存）。
/// <para>拒绝响应统一走 <see cref="MiddlewareResponseWriter.WriteRejectedAsync"/>（C270 起纯文本改 JSON <c>{Code, Message}</c>；系统未上线，无存量消费者）。</para>
/// </summary>
public class HubTenantMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// 匿名 WebHook 请求传递租户标识的请求头。
    /// </summary>
    public const string WebHookTenantIdHeader = "GameFrameX-Tenant-Id";

    /// <summary>
    /// 匿名 WebHook 路由前缀（无租户头时返回 401）。
    /// </summary>
    private const string WebHookPathPrefix = "/api/TapdWebHook";

    public HubTenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            // 拒绝超级管理员
            var accountTypeText = user.FindFirst(ClaimConst.AccountType)?.Value;
            if (int.TryParse(accountTypeText, out var accountType)
                && accountType == (int)AccountTypeEnum.SuperAdmin)
            {
                await MiddlewareResponseWriter.WriteRejectedAsync(context, StatusCodes.Status403Forbidden, "超级管理员账号不允许调用 Hub.Api");
                return;
            }

            // 从 JWT 解析租户 Id
            var tenantIdText = user.FindFirst(ClaimConst.TenantId)?.Value;
            if (!long.TryParse(tenantIdText, out var tenantId) || tenantId <= 0)
            {
                await MiddlewareResponseWriter.WriteRejectedAsync(context, StatusCodes.Status403Forbidden, "缺少有效的租户标识");
                return;
            }

            context.Items[TenantContextAccessor.TenantIdKey] = tenantId;
        }
        else
        {
            // 匿名（WebHook）：从请求头解析租户 Id；WebHook 路由无租户头 → 401
            if (context.Request.Headers.TryGetValue(WebHookTenantIdHeader, out var headerValue)
                && long.TryParse(headerValue, out var headerTenantId)
                && headerTenantId > 0)
            {
                context.Items[TenantContextAccessor.TenantIdKey] = headerTenantId;
            }
            else if (context.Request.Path.StartsWithSegments(WebHookPathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                await MiddlewareResponseWriter.WriteRejectedAsync(context, StatusCodes.Status401Unauthorized, "缺少租户标识");
                return;
            }
        }

        await _next(context);
    }
}