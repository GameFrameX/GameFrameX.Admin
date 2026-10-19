using GameFrameX.Core.Service.Auth;

namespace GameFrameX.Core.Tests.System;

public class LoginAccountTenantSuffixTests
{
#pragma warning disable CS8625 // 测试意图：第 3 参数 tenantHost=null 模拟未提供租户域名，验证仅按账号/租户 ID 解析
    [Theory]
    [InlineData("admin@1001", "admin", 1001)]
    [InlineData("user@example.com@1001", "user@example.com", 1001)]
    public void LoginAccountTenantSuffix_ValidNumericSuffix_SplitsFromLastAt(string rawAccount, string expectedAccount, long expectedTenantId)
    {
        var result = SystemAuthService.ResolveLoginAccount(rawAccount, null, null);

        Assert.Equal(expectedAccount, result.Account);
        Assert.Equal(expectedTenantId, result.TenantId);
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("admin@abc")]
    [InlineData("admin@0")]
    [InlineData("@1001")]
    public void LoginAccountTenantSuffix_InvalidOrEmailOnlySuffix_KeepsOriginalAccount(string rawAccount)
    {
        var result = SystemAuthService.ResolveLoginAccount(rawAccount, null, null);

        Assert.Equal(rawAccount, result.Account);
        Assert.Null(result.TenantId);
    }

    [Fact]
    public void LoginAccountTenantSuffix_ExplicitTenantId_WinsOverSuffix()
    {
        var result = SystemAuthService.ResolveLoginAccount("admin@1001", 2002, null);

        Assert.Equal("admin@1001", result.Account);
        Assert.Equal(2002, result.TenantId);
    }

    [Fact]
    public void LoginAccountTenantSuffix_TenantHost_WinsOverSuffix()
    {
        var result = SystemAuthService.ResolveLoginAccount("admin@1001", null, "tenant.example.com");

        Assert.Equal("admin@1001", result.Account);
        Assert.Null(result.TenantId);
    }
#pragma warning restore CS8625
}