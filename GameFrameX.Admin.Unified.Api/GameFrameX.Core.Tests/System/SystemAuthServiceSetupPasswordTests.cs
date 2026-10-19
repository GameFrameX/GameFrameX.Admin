using System.Net;
using GameFrameX.Core.Enum;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Service.Auth.Dto;
using GameFrameX.Core.Service.PlatformPasswordPolicy;
using GameFrameX.Core.Utility;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// 平台超管首装密码初始化（SetupSuperAdminPassword）守护测试。
/// <para>锁定：①未配置窗口内可设置（Argon2id 落库 + 自动登录返回令牌）；②已配置后 fail-closed（30000018）；
/// ③两次输入不一致拒绝；④平台强密码策略开启时弱密码拒绝；⑤未配置超管密码登录被引导（30000017）；
/// ⑥GetLoginConfig 如实上报 SuperAdminPasswordConfigured。</para>
/// </summary>
public class SystemAuthServiceSetupPasswordTests
{
    private static void UseUniqueClientIp(AuthTestHost host)
    {
        // 进程级静态限流字典按 IP 隔离，每个测试独立 IP 避免相互触发间隔限制
        host.HttpContext.Connection.RemoteIpAddress = IPAddress.Parse($"10.{Random.Shared.Next(1, 255)}.{Random.Shared.Next(0, 255)}.{Random.Shared.Next(1, 254)}");
    }

    [Fact]
    public async Task Setup_NotConfigured_SetsArgon2idPasswordAndAutoLogin()
    {
        using var host = new AuthTestHost();
        UseUniqueClientIp(host);
        var svc = host.BuildService();
        await host.SeedSuperAdminAsync("superadmin", "123456", passwordConfigured: false);

        var output = await svc.SetupSuperAdminPasswordAsync(new SetupSuperAdminPasswordRequest
        {
            Password = "Abc12345",
            ConfirmPassword = "Abc12345",
        });

        // 自动登录：复用 CreateTokenAsync 链（Fake Jwt 返回固定令牌 + SuperAdmin 身份）
        Assert.Equal("fake-access-token", output.AccessToken);
        Assert.Equal("fake-refresh-token", output.RefreshToken);

        // 落库：Argon2id 哈希 + 已配置 + 新密码可验过
        var superAdmin = await host.GetSuperAdminAsync();
        Assert.NotNull(superAdmin);
        Assert.True(superAdmin!.PasswordConfigured);
        Assert.StartsWith("$argon2id$", superAdmin.Password);
        Assert.True(CryptogramUtility.VerifyPassword("Abc12345", superAdmin.Password));
    }

    [Fact]
    public async Task Setup_AlreadyConfigured_ThrowsFailClosed()
    {
        using var host = new AuthTestHost();
        UseUniqueClientIp(host);
        var svc = host.BuildService();
        // 字面播种「已配置」态（GFX-747 D4）：显式传 true 而非依赖默认参数，
        // 测 fail-closed 真实路径——WHERE password_configured == false 不命中 → 0 行更新 → 30000018
        await host.SeedSuperAdminAsync("superadmin", "123456", passwordConfigured: true);

        var ex = await Assert.ThrowsAsync<BusinessException>(() => svc.SetupSuperAdminPasswordAsync(new SetupSuperAdminPasswordRequest
        {
            Password = "Abc12345",
            ConfirmPassword = "Abc12345",
        }));

        // fail-closed：已配置（无论向导设置或 seed 已配置）永久拒绝，且原密码不被覆盖
        Assert.Equal(SystemUserErrorCode.SuperAdminPasswordAlreadyConfigured, (SystemUserErrorCode)ex.ErrorCode!);
        var superAdmin = await host.GetSuperAdminAsync();
        Assert.True(CryptogramUtility.VerifyPassword("123456", superAdmin!.Password));
    }

    [Fact]
    public async Task Setup_MismatchedConfirm_Throws()
    {
        using var host = new AuthTestHost();
        UseUniqueClientIp(host);
        var svc = host.BuildService();
        await host.SeedSuperAdminAsync("superadmin", "123456", passwordConfigured: false);

        var ex = await Assert.ThrowsAsync<BusinessException>(() => svc.SetupSuperAdminPasswordAsync(new SetupSuperAdminPasswordRequest
        {
            Password = "Abc12345",
            ConfirmPassword = "Abc54321",
        }));

        Assert.Equal(SystemUserErrorCode.SetupPasswordConfirmMismatch, (SystemUserErrorCode)ex.ErrorCode!);
        var superAdmin = await host.GetSuperAdminAsync();
        Assert.False(superAdmin!.PasswordConfigured);
    }

    [Fact]
    public async Task Setup_StrongPasswordEnabled_WeakPasswordThrows()
    {
        var originalStrongPassword = CryptogramUtility.StrongPassword;
        var originalValidation = CryptogramUtility.PasswordStrengthValidation;
        var originalMessage = CryptogramUtility.PasswordStrengthValidationMsg;
        try
        {
            CryptogramUtility.ApplyPasswordPolicy(true,
                PlatformPasswordPolicyConfigConstants.DefaultPasswordStrengthValidation,
                PlatformPasswordPolicyConfigConstants.DefaultPasswordStrengthValidationMsg);

            using var host = new AuthTestHost();
            UseUniqueClientIp(host);
            var svc = host.BuildService();
            await host.SeedSuperAdminAsync("superadmin", "123456", passwordConfigured: false);

            var ex = await Assert.ThrowsAsync<BusinessException>(() => svc.SetupSuperAdminPasswordAsync(new SetupSuperAdminPasswordRequest
            {
                Password = "abc123",
                ConfirmPassword = "abc123",
            }));

            Assert.Equal(PlatformPasswordPolicyConfigConstants.DefaultPasswordStrengthValidationMsg, ex.Message);
            var superAdmin = await host.GetSuperAdminAsync();
            Assert.False(superAdmin!.PasswordConfigured);
        }
        finally
        {
            CryptogramUtility.ApplyPasswordPolicy(originalStrongPassword, originalValidation, originalMessage);
        }
    }

    [Fact]
    public async Task Login_NotConfiguredSuperAdmin_ThrowsGuideError()
    {
        using var host = new AuthTestHost();
        UseUniqueClientIp(host);
        var svc = host.BuildService();
        await host.SeedSuperAdminAsync("superadmin", "123456", passwordConfigured: false);

        var ex = await Assert.ThrowsAsync<BusinessException>(() => svc.LoginAsync(new LoginRequest
        {
            Account = "superadmin",
            Password = "123456",
        }));

        // 未配置窗口内密码登录被拒并引导设置，不给"账号或密码错误"误导
        Assert.Equal(SystemUserErrorCode.SuperAdminPasswordNotConfigured, (SystemUserErrorCode)ex.ErrorCode!);
    }

    [Fact]
    public async Task GetLoginConfig_ReportsSuperAdminPasswordConfiguredState()
    {
        using var host = new AuthTestHost();
        UseUniqueClientIp(host);
        var svc = host.BuildService();

        // 未配置：向导应展示
        await host.SeedSuperAdminAsync("superadmin", "123456", passwordConfigured: false);
        var pending = await svc.GetLoginConfigAsync();
        Assert.False(pending.Data.SuperAdminPasswordConfigured);

        // 配置完成：向导收起
        await svc.SetupSuperAdminPasswordAsync(new SetupSuperAdminPasswordRequest
        {
            Password = "Abc12345",
            ConfirmPassword = "Abc12345",
        });
        var configured = await svc.GetLoginConfigAsync();
        Assert.True(configured.Data.SuperAdminPasswordConfigured);
    }
}
