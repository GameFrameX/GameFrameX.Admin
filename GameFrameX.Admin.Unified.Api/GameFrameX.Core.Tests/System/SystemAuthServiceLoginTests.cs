using GameFrameX.Core.Const;
using GameFrameX.Core.Enum;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Service.Auth.Dto;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// SystemAuthService 登录端到端集成测试。
/// <para>C217 part 2a 覆盖核心场景 ①②③（不存在/错误/响应一致——C216 枚举防护端到端）。</para>
/// <para>C218 补充 4 场景：③正确密码成功（CreateTokenAsync + Claim）/ ④账号冻结 D1017 / ⑤租户禁用 Z1003 / ⑥连续失败锁定。</para>
/// </summary>
public class SystemAuthServiceLoginTests
{
    [Fact]
    public async Task Login_NonExistentAccount_ThrowsUnifiedAccountOrPasswordError()
    {
        using var host = new AuthTestHost();
        var svc = host.BuildService();
        await host.SeedSuperAdminAsync("superadmin", "123456");

        var ex = await Assert.ThrowsAsync<BusinessException>(() => svc.LoginAsync(new LoginRequest
        {
            Account = "nobody",
            Password = "whatever",
        }));

        Assert.Null(ex.ErrorCode);
        Assert.Equal("账号或密码错误", ex.Message);
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsUnifiedAccountOrPasswordError()
    {
        using var host = new AuthTestHost();
        var svc = host.BuildService();
        await host.SeedSuperAdminAsync("superadmin", "123456");

        var ex = await Assert.ThrowsAsync<BusinessException>(() => svc.LoginAsync(new LoginRequest
        {
            Account = "superadmin",
            Password = "wrong-password",
        }));

        Assert.Null(ex.ErrorCode);
        Assert.Equal("账号或密码错误", ex.Message);
    }

    [Fact]
    public async Task Login_NonExistent_And_WrongPassword_Produce_IdenticalResponse()
    {
        // C216 枚举防护端到端核心断言：不存在账号（假 Verify）与错误密码响应完全一致，不可区分
        using var host = new AuthTestHost();
        var svc = host.BuildService();
        await host.SeedSuperAdminAsync("superadmin", "123456");

        var noneEx = await Assert.ThrowsAsync<BusinessException>(() => svc.LoginAsync(new LoginRequest { Account = "ghost", Password = "x", }));
        var wrongEx = await Assert.ThrowsAsync<BusinessException>(() => svc.LoginAsync(new LoginRequest { Account = "superadmin", Password = "x", }));

        Assert.Equal(noneEx.ErrorCode?.ToString(), wrongEx.ErrorCode?.ToString());
        Assert.Equal(noneEx.Message, wrongEx.Message);
    }

    [Fact]
    public async Task Login_CorrectPassword_ReturnsTokenWithClaims()
    {
        // 场景 ③正确密码成功：走 CreateTokenAsync 链（GetTokenExpireAsync 默认 20 + Fake Jwt 返回 token + Claim 注入）
        using var host = new AuthTestHost();
        var svc = host.BuildService();
        await host.SeedSuperAdminAsync("superadmin", "123456");

        var output = await svc.LoginAsync(new LoginRequest
        {
            Account = "superadmin",
            Password = "123456",
        });

        Assert.Equal("fake-access-token", output.AccessToken);
        Assert.Equal("fake-refresh-token", output.RefreshToken);
        // Claim 验证：CreateTokenAsync 注入的 payload 携带 SuperAdmin 身份
        Assert.Equal(1L, host.Jwt.LastAccessTokenPayload![ClaimConst.UserId]);
        Assert.Equal("superadmin", host.Jwt.LastAccessTokenPayload![ClaimConst.Account]);
        Assert.Equal(AccountTypeEnum.SuperAdmin, host.Jwt.LastAccessTokenPayload![ClaimConst.AccountType]);
    }

    [Fact]
    public async Task Login_FrozenAccount_ThrowsD1017()
    {
        // 场景 ④账号冻结：seed superadmin IsEnabled=false → D1017（密码校验前即抛出）
        using var host = new AuthTestHost();
        var svc = host.BuildService();
        await host.SeedSuperAdminAsync("superadmin", "123456", false);

        var ex = await Assert.ThrowsAsync<BusinessException>(() => svc.LoginAsync(new LoginRequest
        {
            Account = "superadmin",
            Password = "123456",
        }));

        Assert.Equal(SystemUserErrorCode.AccountFrozen, (SystemUserErrorCode)ex.ErrorCode!);
    }

    [Fact]
    public async Task Login_TenantDisabled_ThrowsZ1003()
    {
        // 场景 ⑤租户禁用：租户库 seed TenantUser + 主库 seed SystemTenant(IsEnabled=false) → Z1003
        using var host = new AuthTestHost();
        var svc = host.BuildService();
        await host.SeedTenantAsync(AuthTestHost.SeedTenantId, false);
        await host.SeedTenantUserAsync(AuthTestHost.SeedTenantId, "tenantuser", "123456");

        var ex = await Assert.ThrowsAsync<BusinessException>(() => svc.LoginAsync(new LoginRequest
        {
            Account = "tenantuser",
            Password = "123456",
            TenantId = AuthTestHost.SeedTenantId,
        }));

        Assert.Equal(TenantErrorCode.TenantDisabled, (TenantErrorCode)ex.ErrorCode!);
    }

    [Fact]
    public async Task Login_RepeatedFailures_LockoutAfterMaxAttempts()
    {
        // 场景 ⑥锁定：unique 账号隔离 static _loginFailCache，连续 MaxLoginFailCount 次 → 锁定
        var account = $"lock-{Guid.NewGuid():N}";
        using var host = new AuthTestHost();
        var svc = host.BuildService();

        // 前 MaxLoginFailCount-1 次（4 次）：统一"账号或密码错误"
        for (var i = 0; i < 4; i++)
        {
            var ex = await Assert.ThrowsAsync<BusinessException>(() => svc.LoginAsync(new LoginRequest
            {
                Account = account,
                Password = "whatever",
            }));
            Assert.Equal("账号或密码错误", ex.Message);
        }

        // 第 5 次达阈值：锁定提示
        var lockoutEx = await Assert.ThrowsAsync<BusinessException>(() => svc.LoginAsync(new LoginRequest
        {
            Account = account,
            Password = "whatever",
        }));
        Assert.Contains("已锁定", lockoutEx.Message);
    }
}