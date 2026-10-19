using GameFrameX.Core.Job;
using GameFrameX.Application.System.Job;
using GameFrameX.Core.Api;
using GameFrameX.Core.DependencyInjection;
using GameFrameX.Core.Service.Tenant;
using Microsoft.Extensions.DependencyInjection;

namespace GameFrameX.Web.Api.DependencyInjection;

public static class ProjectDependencyInjectionSetup
{
    private static readonly HashSet<string> SingletonTypes = new(StringComparer.Ordinal)
    {
        "GameFrameX.Core.EventBus.AppEventSubscriber",
"GameFrameX.Wechat.Abstract.WechatApiHttpClientFactory",
        "GameFrameX.Core.Service.Cache.SystemCacheService",
        "GameFrameX.Core.Job.DynamicJobCompiler",
        "GameFrameX.Core.Service.PlatformCaptcha.PlatformCaptchaStorage",
        "GameFrameX.Core.Service.PlatformRateLimit.PlatformRateLimitConfigProvider",
    };

    private static readonly HashSet<string> ScopedTypes = new(StringComparer.Ordinal)
    {
        "GameFrameX.Core.Service.User.CurrentUserContext",
        "GameFrameX.Core.Filter.TenantExpirationFilter",
        "GameFrameX.Application.Game.Tenant.ClientApiPolicy.RiskEnforcement.GameAppRiskEnforcementModeResolver",
        "GameFrameX.Application.Game.Penalty.GamePenalty.TenantGamePenaltyEnforcementService",
        "GameFrameX.Application.Game.User.GameUser.TenantGameUserPasswordService",
    };

    /// <summary>
    /// 注册项目生产服务类型，替代 历史框架 DI marker 自动注册。
    /// </summary>
    public static IServiceCollection AddProjectDependencyInjection(this IServiceCollection services)
    {
        foreach (var implementationType in ProjectTypeCatalog.EffectiveTypes.Where(IsServiceImplementation))
        {
            var lifetime = ResolveLifetime(implementationType);
            AddServiceDescriptor(services, implementationType, implementationType, lifetime);

            foreach (var serviceType in implementationType.GetInterfaces().Where(IsProjectServiceInterface))
            {
                AddServiceDescriptor(services, serviceType, implementationType, lifetime);
            }
        }

        AddServiceDescriptor(services, typeof(IJobClusterServer), typeof(JobClusterServer), ServiceLifetime.Singleton);
        AddServiceDescriptor(services, typeof(TenantAccessGate), typeof(TenantAccessGate), ServiceLifetime.Transient);
        AddServiceDescriptor(services, typeof(PostgreSqlTenantDatabaseProvisioner), typeof(PostgreSqlTenantDatabaseProvisioner), ServiceLifetime.Transient);

        return services;
    }

    private static bool IsServiceImplementation(Type type)
    {
        return type is { IsClass: true, IsAbstract: false, IsPublic: true, }
               && !type.ContainsGenericParameters
               && IsApplicationAssembly(type.Assembly.GetName().Name)
               && (typeof(IAdminApiController).IsAssignableFrom(type)
                   || IsServiceLikeName(type.Name)
                   || SingletonTypes.Contains(type.FullName ?? string.Empty)
                   || ScopedTypes.Contains(type.FullName ?? string.Empty));
    }

    private static bool IsApplicationAssembly(string? assemblyName)
    {
        return assemblyName is "GameFrameX.Application"
                   or "GameFrameX.Application.Game"
                   or "GameFrameX.Application.System"
                   or "GameFrameX.Core"
                   or "GameFrameX.Core.Utility"
                   or "GameFrameX.ObjectStorage.Local"
                   or "GameFrameX.Wechat.Abstract";
    }

    private static bool IsServiceLikeName(string name)
    {
        return name.EndsWith("Service", StringComparison.Ordinal)
               || name.EndsWith("Provider", StringComparison.Ordinal)
               || name.EndsWith("Resolver", StringComparison.Ordinal)
               || name.EndsWith("Helper", StringComparison.Ordinal)
               || name.EndsWith("Factory", StringComparison.Ordinal)
               || name.EndsWith("Mapper", StringComparison.Ordinal)
               || name.EndsWith("Compiler", StringComparison.Ordinal)
               || name.EndsWith("Subscriber", StringComparison.Ordinal)
               || name.EndsWith("Runner", StringComparison.Ordinal)
               || name.EndsWith("Filter", StringComparison.Ordinal);
    }

    private static ServiceLifetime ResolveLifetime(Type type)
    {
        var fullName = type.FullName ?? string.Empty;
        if (SingletonTypes.Contains(fullName))
        {
            return ServiceLifetime.Singleton;
        }

        if (ScopedTypes.Contains(fullName))
        {
            return ServiceLifetime.Scoped;
        }

        return ServiceLifetime.Transient;
    }

    private static bool IsProjectServiceInterface(Type type)
    {
        return type is { IsInterface: true, IsGenericTypeDefinition: false, }
               && type != typeof(IDisposable)
               && type != typeof(IAsyncDisposable)
               && type != typeof(IAdminApiController)
               && type.Namespace?.StartsWith("GameFrameX.", StringComparison.Ordinal) == true;
    }

    private static void AddServiceDescriptor(IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime)
    {
        if (services.Any(descriptor => descriptor.ServiceType == serviceType && descriptor.ImplementationType == implementationType))
        {
            return;
        }

        services.Add(new ServiceDescriptor(serviceType, implementationType, lifetime));
    }
}