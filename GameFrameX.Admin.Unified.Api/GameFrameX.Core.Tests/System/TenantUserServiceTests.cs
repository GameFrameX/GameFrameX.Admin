using GameFrameX.Core.Service.User;

namespace GameFrameX.Core.Tests.System;

public class TenantUserServiceTests
{
    [Fact]
    public void NormalizeNewUser_DefaultsNullStringColumns()
    {
        var user = new TenantUserEntity { Account = "demo", };

        TenantUserService.NormalizeNewUser(user);

        foreach (var property in typeof(TenantUserEntity).GetProperties().Where(u => u.PropertyType == typeof(string) && u.CanWrite))
        {
            Assert.NotNull(property.GetValue(user));
        }

        Assert.Equal("demo", user.Account);
        Assert.Equal(string.Empty, user.Avatar);
        Assert.Equal(string.Empty, user.Nation);
    }
}