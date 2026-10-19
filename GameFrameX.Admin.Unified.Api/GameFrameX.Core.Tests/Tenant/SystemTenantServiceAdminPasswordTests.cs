using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.Utility;

namespace GameFrameX.Core.Tests.Tenant;

/// <summary>
/// SystemTenantService 租管密码解析纯逻辑单测（不依赖数据库）。
/// </summary>
public class SystemTenantServiceAdminPasswordTests
{
    [Fact]
    public void ResolveTenantAdminEncryptedPassword_OverrideWins()
    {
        var result = SystemTenantService.ResolveTenantAdminEncryptedPassword("$argon2id$v=19$override-ciphertext", "123456");

        Assert.Equal("$argon2id$v=19$override-ciphertext", result);
    }

    [Fact]
    public void ResolveTenantAdminEncryptedPassword_NullOverride_HashesConfiguredPassword()
    {
        var result = SystemTenantService.ResolveTenantAdminEncryptedPassword(null, "123456");

        Assert.NotEqual("123456", result);
        Assert.True(CryptogramUtility.VerifyPassword("123456", result));
    }
}
