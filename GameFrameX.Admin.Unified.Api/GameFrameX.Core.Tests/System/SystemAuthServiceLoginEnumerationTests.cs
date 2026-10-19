using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Service.Auth;
using GameFrameX.Core.Utility;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// Admin 登录账号枚举防护单测（C216）：RecordLoginFailure 计数/锁定统一响应 + EnumerationDecoyHash 诱饵合法性。
/// <para>user null 与密码错误共用 RecordLoginFailure，响应不可区分（ErrorCode 均为 null、Message 统一）。</para>
/// <para>耗时对齐（假 Verify）属运行时行为，由代码审查 + 登录端到端验收覆盖，不在本单测度量。</para>
/// </summary>
public class SystemAuthServiceLoginEnumerationTests
{
    private static string Unique()
    {
        return $"test-{Guid.NewGuid():N}";
    }

    [Fact]
    public void RecordLoginFailure_BelowThreshold_ThrowsUnifiedAccountOrPasswordError()
    {
        var ex = Assert.Throws<BusinessException>(() => SystemAuthService.RecordLoginFailure(Unique(), Unique()));

        Assert.Null(ex.ErrorCode); // 字符串消息创建，无错误码（与密码错误一致，不暴露账号存在性）
        Assert.Equal("账号或密码错误", ex.Message);
    }

    [Fact]
    public void RecordLoginFailure_AtThreshold_ThrowsLockout_AfterMaxAttempts()
    {
        var failKey = Unique();
        var lockoutKey = Unique();

        // 前 MaxLoginFailCount-1 次（4 次）：failCount 1..4，未达阈值，抛统一"账号或密码错误"
        for (var i = 0; i < 4; i++)
        {
            var ex = Assert.Throws<BusinessException>(() => SystemAuthService.RecordLoginFailure(failKey, lockoutKey));
            Assert.Equal("账号或密码错误", ex.Message);
        }

        // 第 MaxLoginFailCount 次（第 5 次，failCount=5 >= 5）：抛锁定提示
        var lockoutEx = Assert.Throws<BusinessException>(() => SystemAuthService.RecordLoginFailure(failKey, lockoutKey));
        Assert.Null(lockoutEx.ErrorCode);
        Assert.Contains("锁定", lockoutEx.Message);
        Assert.Contains("30", lockoutEx.Message); // LockoutDuration = 30 min
    }

    [Fact]
    public void EnumerationDecoyHash_IsValidArgon2id_AndVerifyReturnsFalse()
    {
        // 诱饵哈希合法 Argon2id PHC，假比对返回 false（不匹配任意真实密码）
        Assert.StartsWith("$argon2id$", CryptogramUtility.EnumerationDecoyHash);
        Assert.False(CryptogramUtility.VerifyPassword("any-input-password", CryptogramUtility.EnumerationDecoyHash));
        Assert.False(CryptogramUtility.VerifyPassword(string.Empty, CryptogramUtility.EnumerationDecoyHash));
    }

    [Fact]
    public void EnumerationDecoyHash_DoesNotMatchTypicalUserPasswords()
    {
        // 诱饵不匹配常见密码，确保假 Verify 路径总返回 false（user null 路径走假比对，结果被丢弃）
        foreach (var pwd in new[] { "123456", "password", "admin", "right-password", })
        {
            Assert.False(CryptogramUtility.VerifyPassword(pwd, CryptogramUtility.EnumerationDecoyHash));
        }
    }
}