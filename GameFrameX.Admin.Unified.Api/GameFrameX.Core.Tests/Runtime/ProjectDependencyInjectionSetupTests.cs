using GameFrameX.Core.Job;
using GameFrameX.Application.System.Job;
using GameFrameX.Application.System.Plugin;
using GameFrameX.Wechat.Abstract;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.Service.User;
using GameFrameX.Foundation.Localization.Providers;
using GameFrameX.Web.Api.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace GameFrameX.Core.Tests.Runtime;

/// <summary>
/// C40：项目自有 DI 扫描规则替代 历史框架 marker 的最小注册契约。
/// </summary>
public class ProjectDependencyInjectionSetupTests
{
    [Fact]
    public void AddProjectDependencyInjection_RegistersKeyServicesWithExpectedLifetimes()
    {
        var services = new ServiceCollection();

        services.AddProjectDependencyInjection();

        AssertDescriptor(services, typeof(TenantUserService), typeof(TenantUserService), ServiceLifetime.Transient);
        AssertDescriptor(services, typeof(TenantWechatConfigResolver), typeof(TenantWechatConfigResolver), ServiceLifetime.Transient);
        AssertDescriptor(services, typeof(SystemPluginService), typeof(SystemPluginService), ServiceLifetime.Transient);
        AssertDescriptor(services, typeof(DynamicJobCompiler), typeof(DynamicJobCompiler), ServiceLifetime.Singleton);
        AssertDescriptor(services, typeof(IJobClusterServer), typeof(JobClusterServer), ServiceLifetime.Singleton);
        AssertDescriptor(services, typeof(TenantAccessGate), typeof(TenantAccessGate), ServiceLifetime.Transient);
        AssertDescriptor(services, typeof(PostgreSqlTenantDatabaseProvisioner), typeof(PostgreSqlTenantDatabaseProvisioner), ServiceLifetime.Transient);
    }

    [Fact]
    public void AddProjectDependencyInjection_DoesNotScanFoundationLibraryServices()
    {
        var services = new ServiceCollection();

        services.AddProjectDependencyInjection();

        Assert.DoesNotContain(services, descriptor => descriptor.ImplementationType == typeof(AssemblyResourceProvider));
    }

    [Fact]
    public void AddProjectDependencyInjection_IsIdempotentForConcreteDescriptors()
    {
        var services = new ServiceCollection();

        services.AddProjectDependencyInjection();
        services.AddProjectDependencyInjection();

        var count = services.Count(descriptor =>
                                       descriptor.ServiceType == typeof(DynamicJobCompiler)
                                       && descriptor.ImplementationType == typeof(DynamicJobCompiler));

        Assert.Equal(1, count);
    }

    private static void AssertDescriptor(IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime)
    {
        var descriptor = services.SingleOrDefault(item =>
                                                      item.ServiceType == serviceType
                                                      && item.ImplementationType == implementationType);

        Assert.NotNull(descriptor);
        Assert.Equal(lifetime, descriptor!.Lifetime);
    }
}