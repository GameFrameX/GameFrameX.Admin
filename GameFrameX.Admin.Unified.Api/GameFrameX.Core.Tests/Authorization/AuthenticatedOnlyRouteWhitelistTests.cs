using System.Reflection;
using GameFrameX.Core.Authorization;
using GameFrameX.Core.Filter;
using GameFrameX.Core.Service.Auth;

namespace GameFrameX.Core.Tests.Authorization;

/// <summary>
/// 登录态端点由 AuthenticatedOnlyAttribute 声明登记，避免退出与刷新等基础动作依赖业务菜单权限。
/// 生产代码不得重建手写路由白名单。
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
            };
        }
    }

    /// <summary>
    /// 仅登录端点必须显式标注 [AuthenticatedOnly]，
    /// 端点仍需落在「仅需登录」档，否则普通管理员会被自动隐藏按钮挡成 403。
    /// </summary>
    [Theory]
    [MemberData(nameof(EscapeHatchActions))]
    public void EscapeHatchEndpoint_MustBeAuthenticatedOnly(Type serviceType, string methodName)
    {
        var method = serviceType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);
        Assert.True(method!.IsDefined(typeof(AuthenticatedOnlyAttribute), false),
                    $"{serviceType.Name}.{methodName} 未标注 [AuthenticatedOnly]，运营账号会被自动隐藏按钮挡成 403");
    }

    /// <summary>
    /// 防回潮：Web.Api/Handlers 生产代码不得出现登录态白名单手写路由清单/中转表标识残留。
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
