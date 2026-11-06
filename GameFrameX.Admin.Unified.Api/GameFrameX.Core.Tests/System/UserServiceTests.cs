using GameFrameX.Core.Service.User;

namespace GameFrameX.Core.Tests.System;

public class UserServiceTests
{
    [Fact]
    public void NormalizeNewUser_DefaultsNullStringColumns()
    {
        var user = new UserEntity { Account = "demo", };

        UserService.NormalizeNewUser(user);

        foreach (var property in typeof(UserEntity).GetProperties().Where(u => u.PropertyType == typeof(string) && u.CanWrite))
        {
            Assert.NotNull(property.GetValue(user));
        }

        Assert.Equal("demo", user.Account);
        Assert.Equal(string.Empty, user.Avatar);
        Assert.Equal(string.Empty, user.Nation);
    }
}