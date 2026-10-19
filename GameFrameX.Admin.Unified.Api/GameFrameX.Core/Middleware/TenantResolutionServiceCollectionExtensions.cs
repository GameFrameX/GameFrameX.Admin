namespace GameFrameX.Core.Middleware;

/// <summary>
/// 租户解析接线扩展（Client / Game / Hub 三 host 共用，C260 收敛）：
/// 一次完成 <c>IHttpContextAccessor</c> 与读取统一租户键的 <c>ISqlSugarTenantIdResolver</c> 的注册。
/// </summary>
public static class TenantResolutionServiceCollectionExtensions
{
    /// <summary>
    /// 注册租户解析接线：宿主创建的 <see cref="IHttpContextAccessor"/> 单例 +
    /// 从 <see cref="TenantContextAccessor.TenantIdKey"/> 读取租户 Id 的 <see cref="ISqlSugarTenantIdResolver"/> 单例。
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="httpContextAccessor">宿主创建的 HttpContext 访问器（与请求管线共享同一实例）。</param>
    /// <returns>已注册的解析器实例，供宿主 SqlSugarClientConfig 复用同一实例，避免重复注册。</returns>
    public static DelegateSqlSugarTenantIdResolver AddSqlSugarTenantResolution(this IServiceCollection services, IHttpContextAccessor httpContextAccessor)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (httpContextAccessor == null)
        {
            throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        services.AddSingleton(httpContextAccessor);
        var tenantIdResolver = new DelegateSqlSugarTenantIdResolver(() => TenantContextAccessor.GetTenantId(httpContextAccessor.HttpContext));
        services.AddSingleton<ISqlSugarTenantIdResolver>(tenantIdResolver);
        return tenantIdResolver;
    }
}