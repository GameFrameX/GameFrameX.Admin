using System.Reflection;
using GameFrameX.Core.Authorization;
using GameFrameX.Core.Filter;
using GameFrameX.Core.Service.Auth;
using GameFrameX.Core.Service.TenantPackage;

namespace GameFrameX.Core.Tests.Authorization;

/// <summary>
/// 登录态白名单 ↔ 逃生通道端点对齐守护（C275 / GFX-683；C277 / GFX-684 特性化改造）。
/// <para>「仅需登录」档已收敛为 <see cref="AuthenticatedOnlyAttribute"/> 声明即登记，
/// 不再有手写路由清单。租户到期门禁（TenantExpirationFilter）的非匿名逃生通道端点
/// （Logout / GetRefreshTokenAsync / GetTenantExpirationStatusAsync）必须同时标注 <c>[AuthenticatedOnly]</c>——
/// 否则被自动隐藏按钮挡成 403（自动按钮默认不授权任何租户角色），到期租户用户将被锁死在系统内。</para>
/// <para>另含防回潮守护：Web.Api/Handlers 生产代码不得重建路由字符串静态清单（GFX-684）。</para>
/// </summary>
public sealed class AuthenticatedOnlyRouteWhitelistTests
{
    public static TheoryData<Type, string> EscapeHatchActions
    {
        get
        {
            return new TheoryData<Type, string>
            {
                { typeof(SystemAuthService), nameof(SystemAuthService.Logout) },
                { typeof(SystemAuthService), nameof(SystemAuthService.GetRefreshTokenAsync) },
                { typeof(GlobalPackageService), nameof(GlobalPackageService.GetTenantExpirationStatusAsync) },
            };
        }
    }

    /// <summary>
    /// 逃生通道端点必须同时标注 [AuthenticatedOnly]：[BypassTenantExpiration] 只豁免租户门禁，
    /// 端点仍需落在「仅需登录」档，否则 tenant admin 会被自动隐藏按钮挡成 403（GFX-683 回潮）。
    /// </summary>
    [Theory]
    [MemberData(nameof(EscapeHatchActions))]
    public void EscapeHatchEndpoint_MustBeAuthenticatedOnly(Type serviceType, string methodName)
    {
        var method = serviceType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);
        Assert.True(method!.IsDefined(typeof(BypassTenantExpirationAttribute), false),
                    $"{serviceType.Name}.{methodName} 必须标注 [BypassTenantExpiration]，否则禁用/过期租户用户无法使用该逃生通道端点");
        Assert.True(method.IsDefined(typeof(AuthenticatedOnlyAttribute), false),
                    $"{serviceType.Name}.{methodName} 标注了 [BypassTenantExpiration] 却未标注 [AuthenticatedOnly]——tenant admin 会被自动隐藏按钮挡成 403（GFX-683 回潮）");
    }

    /// <summary>
    /// 防回潮：Web.Api/Handlers 生产代码不得出现登录态白名单手写路由清单/中转表标识残留（GFX-684）。
    /// 权限语义必须在方法定义处用特性声明，任何「集中登记路由字符串」的形态都是回潮。
    /// </summary>
    [Fact]
    public void WebApiHandlers_HandWrittenAuthenticatedOnlyRouteRegistry_Absent()
    {
        var handlersDir = FindSourceDirectory("GameFrameX.Web.Api", "Handlers");
        var offenders = new List<string>();
        foreach (var file in Directory.EnumerateFiles(handlersDir, "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                || file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            {
                continue;
            }

            var source = File.ReadAllText(file);
            if (source.Contains("AuthenticatedOnlyRoute", StringComparison.Ordinal)
                || source.Contains("global_authenticated_only_route", StringComparison.Ordinal))
            {
                offenders.Add(Path.GetFileName(file));
            }
        }

        Assert.True(offenders.Count == 0,
                    $"Web.Api/Handlers 生产代码不得重建登录态白名单手写清单（应使用 [AuthenticatedOnly] 特性声明即登记）：{string.Join(", ", offenders)}");
    }

    private static string FindSourceDirectory(string projectName, string subDirectory)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var candidate = Path.Combine(directory.FullName, projectName, subDirectory);
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException($"找不到 {projectName}/{subDirectory} 目录。");
    }
}