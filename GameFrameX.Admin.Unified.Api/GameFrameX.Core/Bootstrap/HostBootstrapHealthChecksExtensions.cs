using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GameFrameX.Core.Bootstrap;

/// <summary>
/// 四 host（Web / Client / Hub / Game）共享的 HealthChecks 启动装配扩展（C261，GFX-668）。
/// <para>收敛各 Program.cs / Startup.cs 内联的 live/ready 检查注册与三端点映射：
/// 存活探针 self（live 标签，恒 Healthy）判进程可用性，就绪探针 database（ready 标签，
/// <see cref="SqlSugarHealthCheck"/> 实测主库连通）判可对外服务。</para>
/// </summary>
public static class HostBootstrapHealthChecksExtensions
{
    /// <summary>
    /// 注册共享健康检查：self（live）+ database（ready）。
    /// </summary>
    /// <param name="services">服务集合。</param>
    public static IServiceCollection AddHostHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(), new[] { "live", })
                .AddCheck<SqlSugarHealthCheck>("database", tags: new[] { "ready", });
        return services;
    }

    /// <summary>
    /// 映射共享健康探针端点：/health 与 /health/live 取 live 标签（存活），/health/ready 取 ready 标签（就绪）。
    /// <para>探针端点统一打 <see cref="HealthProbeProtoVersionExemptMetadata"/>：容器编排 / 负载均衡探针
    /// 不携带 X-Proto-Ver，与公开 S2S 回调同走缺头豁免（Client host 的 ProtoVersionMiddleware）。</para>
    /// </summary>
    /// <param name="endpoints">端点路由构建器。</param>
    /// <param name="allowAnonymous">
    /// 是否对探针端点放行匿名访问。Admin host 有全局鉴权（JwtHandler 对未登记端点 fail-closed），
    /// 探针用于容器编排与负载均衡、不能依赖登录态，须传 true；三游戏 host 无全局鉴权，保持默认 false。
    /// </param>
    public static void MapHostHealthChecks(this IEndpointRouteBuilder endpoints, bool allowAnonymous = false)
    {
        MapSingle(endpoints, "/health", check => check.Tags.Contains("live"), allowAnonymous);
        MapSingle(endpoints, "/health/live", check => check.Tags.Contains("live"), allowAnonymous);
        MapSingle(endpoints, "/health/ready", check => check.Tags.Contains("ready"), allowAnonymous);
    }

    private static void MapSingle(IEndpointRouteBuilder endpoints, string pattern, Func<HealthCheckRegistration, bool> predicate, bool allowAnonymous)
    {
        var builder = endpoints.MapHealthChecks(pattern, new HealthCheckOptions { Predicate = predicate, })
                              .WithMetadata(new HealthProbeProtoVersionExemptMetadata());
        if (allowAnonymous)
        {
            builder.AllowAnonymous();
        }
    }
}

/// <summary>
/// 健康探针端点的协议版本缺头豁免标记：探针来自容器编排 / 负载均衡，不携带 X-Proto-Ver。
/// </summary>
public sealed class HealthProbeProtoVersionExemptMetadata : Middleware.IProtoVersionExempt
{
}