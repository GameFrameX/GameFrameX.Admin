using GameFrameX.Core.Service.PlatformJwt;
using GameFrameX.Core.Enum.Errors;
using GameFrameX.Core.Filter;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace GameFrameX.Web.Api.Handlers;

/// <summary>
/// 平台 JWT 认证注册。
/// </summary>
public static class PlatformJwtAuthenticationSetup
{
    /// <summary>
    /// 注册动态平台 JWT 认证。认证参数在每次请求时从主库 SystemConfigEntity 读取，不依赖 JWT.json。
    /// </summary>
    public static AuthenticationBuilder AddPlatformJwt(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, JwtHandler>();
        services.AddAuthorization(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                         .AddRequirements(new AdminJwtRequirement())
                         .Build();
            // 仅设 DefaultPolicy（供 MVC 全局 AuthorizeFilter 使用），不设 FallbackPolicy。
            // FallbackPolicy 会对所有"无授权元数据的端点"兜底拦截——包括 Swagger 中间件路径
            // （纯中间件、无端点）和 404 路径，是 Swagger 返回 401 的根因。MVC action 改由 Startup
            // 注册的全局 AuthorizeFilter 保护，二者对 MVC 端点效果一致，但文档中间件天然匿名。
            options.DefaultPolicy = policy;
        });

        return services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = async context =>
                {
                    // SignalR WebSocket 握手无法携带 Authorization header，客户端以 ?access_token= 传递
                    // （ASP.NET Core 官方推荐做法）；仅对 Hub 路径放行 query token，其余端点仍只认 header。
                    var accessToken = context.Request.Query["access_token"];
                    if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }
                    var provider = context.HttpContext.RequestServices.GetRequiredService<PlatformJwtRuntimeProvider>();
                    context.Options.TokenValidationParameters = await provider.CreateValidationParametersAsync();
                },
                OnTokenValidated = async context =>
                {
                    var tokenService = context.HttpContext.RequestServices.GetRequiredService<PlatformJwtTokenService>();
                    if (context.Principal == null || !await tokenService.ValidateTokenVersionAsync(context.Principal))
                    {
                        context.Fail("平台JWT令牌版本已失效");
                    }
                },
                OnAuthenticationFailed = context =>
                {
                    context.NoResult();
                    return Task.CompletedTask;
                },
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(AdminResultFilter.CreateStatusCodeResult(StatusCodes.Status401Unauthorized, "401 登录已过期，请重新登录", (int)AuthErrorCode.TokenExpired));
                },
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(AdminResultFilter.CreateStatusCodeResult(StatusCodes.Status403Forbidden, "403 禁止访问，没有权限", (int)AuthErrorCode.Unauthorized));
                },
            };
        });
    }
}