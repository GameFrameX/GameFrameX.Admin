using System.Reflection;
using GameFrameX.Core.Api;
using Microsoft.AspNetCore.Authorization;

namespace GameFrameX.Core.Tests.Architecture;

/// <summary>
/// C258 防回潮契约：Admin host（Web.Api）的 <see cref="IAdminApiController"/> 服务类禁止类级
/// <c>[AllowAnonymous]</c>。类级特性会被 ApiEndpointCatalogSync 收进 action 的 EndpointMetadata，
/// 使该服务全部端点（含 AddAsync/UpdateAsync/DeleteAsync/SetStatusAsync 写操作）登记为 IsPublic，
/// JwtHandler.CheckAuthorizeAsync 对 IsPublic 端点在权限判定前短路放行 → 匿名写可达（GFX-664）。
/// 公开端点必须 action 级标注并经端点目录登记 IsPublic。
/// </summary>
public sealed class AdminClassLevelAllowAnonymousTests
{
    [Fact]
    public void AdminApiControllers_ShouldNotHaveClassLevelAllowAnonymous()
    {
        var offenders = LoadAdminAssemblies()
                        .SelectMany(SafeGetTypes)
                        .Where(IsAdminController)
                        .Where(type => type.GetCustomAttribute<AllowAnonymousAttribute>() != null)
                        .Select(type => type.FullName!)
                        .OrderBy(name => name, StringComparer.Ordinal)
                        .ToList();

        Assert.True(offenders.Count == 0,
                    $"IAdminApiController 服务类禁止类级 [AllowAnonymous]（公开端点须 action 级标注）：{string.Join(", ", offenders)}");
    }

    /// <summary>
    /// 加载测试输出目录下全部 GameFrameX 程序集——与 AdminApiControllerFeatureProvider 的全程序集
    /// IAdminApiController 判据保持一致，不维护易漂移的名单。
    /// </summary>
    private static IEnumerable<Assembly> LoadAdminAssemblies()
    {
        return Directory.EnumerateFiles(AppContext.BaseDirectory, "GameFrameX*.dll")
                        .Select(path => Assembly.Load(AssemblyName.GetAssemblyName(path)))
                        .ToList();
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetExportedTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type != null).Cast<Type>();
        }
    }

    /// <summary>
    /// 与 AdminApiControllerFeatureProvider 同判据：实现 IAdminApiController 的具体 public 类即为 Admin API 控制器。
    /// </summary>
    private static bool IsAdminController(Type type)
    {
        return typeof(IAdminApiController).IsAssignableFrom(type)
               && type.IsClass
               && type.IsPublic
               && !type.IsAbstract
               && !type.ContainsGenericParameters;
    }
}