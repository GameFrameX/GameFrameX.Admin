using Microsoft.AspNetCore.Builder;

namespace GameFrameX.Core.Bootstrap;

/// <summary>
/// 三 host（Client / Hub / Game）共享的 CORS 启动装配扩展（C261，GFX-668）。
/// <para>收敛各 Program.cs 内联的 "CorsPolicy" AllowAnyOrigin 三连注册与 UseCors 调用。
/// Admin host 的 CORS 语义不同（DefaultPolicy + CORS_ORIGINS 环境变量白名单），不适用本扩展。</para>
/// </summary>
public static class HostBootstrapCorsExtensions
{
    /// <summary>
    /// 三 host 统一的 CORS 策略名（AllowAnyOrigin / AllowAnyHeader / AllowAnyMethod）。
    /// </summary>
    public const string CorsPolicyName = "CorsPolicy";

    /// <summary>
    /// 注册共享 "CorsPolicy" 跨域策略：允许所有 Origin、请求头与请求方法（游戏客户端 host 无浏览器同源约束敏感面）。
    /// </summary>
    /// <param name="services">服务集合。</param>
    public static IServiceCollection AddHostCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, configurePolicy =>
            {
                // 允许所有Origin策略
                configurePolicy.AllowAnyOrigin()
                               // 允许任何请求头
                               .AllowAnyHeader()
                               // 允许任何请求方法
                               .AllowAnyMethod();
            });
        });
        return services;
    }

    /// <summary>
    /// 启用共享 "CorsPolicy" 跨域策略；须在 UseAuthentication / UseAuthorization 之前调用以保持既有管线顺序。
    /// </summary>
    /// <param name="app">host 应用。</param>
    public static void UseHostCors(this WebApplication app)
    {
        app.UseCors(CorsPolicyName);
    }
}