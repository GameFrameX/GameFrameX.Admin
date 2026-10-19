using GameFrameX.Core.Authorization;
using GameFrameX.Core.Const;
using GameFrameX.Core.Enum;
using GameFrameX.Core.Service.Config;
using GameFrameX.Core.Service.Menu;
using GameFrameX.Core.Service.PlatformJwt;
using GameFrameX.Core.Utility;
using GameFrameX.Entity.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace GameFrameX.Web.Api.Handlers;

/// <summary>
/// Admin JWT 与菜单权限授权处理器。
/// </summary>
public class JwtHandler : AuthorizationHandler<AdminJwtRequirement>
{
    private readonly IServiceProvider _serviceProvider;

    public JwtHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 自动刷新Token
    /// </summary>
    /// <param name="context"></param>
    /// <param name="requirement"></param>
    /// <returns></returns>
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminJwtRequirement requirement)
    {
        if (_serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext is not DefaultHttpContext httpContext)
        {
            context.Fail();
            return;
        }

        // 获取Token过期时间
        using var serviceScope = _serviceProvider.CreateScope();
        var systemConfigService = serviceScope.ServiceProvider.GetRequiredService<SystemConfigService>();
        var platformJwtTokenService = serviceScope.ServiceProvider.GetRequiredService<PlatformJwtTokenService>();
        var tokenExpire = await systemConfigService.GetTokenExpireAsync();
        var refreshTokenExpire = await systemConfigService.GetRefreshTokenExpireAsync();

        if (await platformJwtTokenService.AutoRefreshTokenAsync(context, httpContext, tokenExpire, refreshTokenExpire) && await CheckAuthorizeAsync(httpContext))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(); // 授权失败
        }
    }

    /// <summary>
    /// 权限校验核心逻辑（U6/C42 接口驱动版）。
    /// <para>判定链：APP 模式放行 → 超管放行 → 端点 RoutePath 查权限目录 → 公开/白名单放行 →
    /// 空绑定拒绝 → 绑定按钮 MenuId 集 ∩ 用户已授权 MenuId 集 非空放行。</para>
    /// <para>授权令牌是按钮 MenuId（稳定），既非路径也非 routeName。详见 ApiEndpointCatalogSync。</para>
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    internal static async Task<bool> CheckAuthorizeAsync(DefaultHttpContext httpContext)
    {
        // 登录模式判断PC、APP
        if (httpContext.User.FindFirst(ClaimConst.LoginMode)?.Value == ((int)LoginModeEnum.APP).ToString())
        {
            return true;
        }

        // 排除超管
        if (httpContext.User.FindFirst(ClaimConst.AccountType)?.Value == ((int)AccountTypeEnum.SuperAdmin).ToString())
        {
            return true;
        }

        // SignalR Hub 连接（WebSocket 握手）不属于 MVC 端点目录，fail-closed 会一律 403；
        // 连接建立按「仅登录」放行，hub 方法级授权由 Hub 内部治理。
        if (httpContext.Request.Path.StartsWithSegments("/hubs"))
        {
            return true;
        }

        var endpointKey = ApiEndpointIdentity.FromRequestEndpointKey(httpContext.Request.Method, httpContext.Request.Path.Value, GetEndpointRouteTemplate(httpContext));
        if (string.IsNullOrEmpty(endpointKey))
        {
            return false;
        }

        var systemMenuService = httpContext.RequestServices.GetRequiredService<GlobalMenuService>();
        var map = await systemMenuService.GetEndpointPermissionMapAsync();
        if (map == null || map.Count == 0 || !map.TryGetValue(endpointKey, out var entry))
        {
            // 未知端点（未登记或目录未就绪）：fail closed。SuperAdmin/APP 已在上方放行。
            return false;
        }

        if (entry.IsPublic || entry.IsAuthenticatedOnly)
        {
            return true;
        }

        // 空绑定 → 拒绝（"权限要进行接口绑定"的强制）。
        var ownMenuIds = await systemMenuService.GetOwnMenuIdListAsync();
        return EndpointAuthorize.HasAnyBoundMenu(entry.BoundMenuIds, ownMenuIds);
    }

    private static string? GetEndpointRouteTemplate(DefaultHttpContext httpContext)
    {
        return (httpContext.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText;
    }
}