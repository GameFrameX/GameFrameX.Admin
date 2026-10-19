using System.Reflection;
using GameFrameX.Core.Filter;
using GameFrameX.Core.Service.Auth;
using GameFrameX.Core.Service.TenantPackage;

namespace GameFrameX.Core.Tests.Tenant;

/// <summary>
/// 锁定租户门禁逃生通道端点清单（<see cref="BypassTenantExpirationAttribute"/>）。
/// <para>这批端点是禁用/过期租户用户的自救通道（登录、登出、刷新 Token、验证码、到期状态提示），
/// 漏标会被 <see cref="TenantExpirationFilter"/> 拦截、把用户锁死在系统内。</para>
/// <para>历史教训：早期白名单按硬编码路径匹配，5 条全部与动态路由推导出的真实路径不符而整体失效；
/// 改为特性绑定后，以本测试锁清单防止误删标注。</para>
/// </summary>
public class TenantExpirationBypassTests
{
    private static void AssertBypassed(Type type, string methodName)
    {
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);
        Assert.True(method.IsDefined(typeof(BypassTenantExpirationAttribute), false),
                    $"{type.Name}.{methodName} 必须标注 [BypassTenantExpiration]，否则禁用/过期租户用户无法使用该逃生通道端点");
    }

    [Fact]
    public void EscapeHatchActions_AreBypassMarked()
    {
        AssertBypassed(typeof(SystemAuthService), nameof(SystemAuthService.LoginAsync));
        AssertBypassed(typeof(SystemAuthService), nameof(SystemAuthService.Logout));
        AssertBypassed(typeof(SystemAuthService), nameof(SystemAuthService.GetRefreshTokenAsync));
        AssertBypassed(typeof(SystemAuthService), nameof(SystemAuthService.GetCaptchaAsync));
        AssertBypassed(typeof(GlobalPackageService), nameof(GlobalPackageService.GetTenantExpirationStatusAsync));
    }
}