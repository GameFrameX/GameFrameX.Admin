using GameFrameX.Core.Enum.Errors;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Service.Auth;
using GameFrameX.Entity.System;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// SystemAuthService 邮箱自助注册验证码纯逻辑单测（不依赖数据库）。
/// </summary>
public class SystemAuthServiceRegisterEmailCodeTests
{
    [Fact]
    public void EnsureRegisterCodeValid_NullCode_ThrowsNotFoundOrExpired()
    {
        var ex = Assert.Throws<BusinessException>(() => SystemAuthService.EnsureRegisterCodeValid(null));

        Assert.Equal(SystemUserErrorCode.EmailCodeNotFoundOrExpired, ex.ErrorCode);
    }

    [Fact]
    public void EnsureRegisterCodeValid_ExpiredCode_ThrowsNotFoundOrExpired()
    {
        var expiredCode = new SystemRegisterEmailCodeEntity
        {
            Email = "admin@example.com",
            Code = "123456",
            ExpirationTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 1,
        };

        var ex = Assert.Throws<BusinessException>(() => SystemAuthService.EnsureRegisterCodeValid(expiredCode));

        Assert.Equal(SystemUserErrorCode.EmailCodeNotFoundOrExpired, ex.ErrorCode);
    }

    [Fact]
    public void EnsureRegisterCodeValid_ValidCode_ReturnsSameEntity()
    {
        var validCode = new SystemRegisterEmailCodeEntity
        {
            Email = "admin@example.com",
            Code = "123456",
            ExpirationTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60,
        };

        var result = SystemAuthService.EnsureRegisterCodeValid(validCode);

        Assert.Same(validCode, result);
    }
}
