using GameFrameX.Core.Utility;

namespace GameFrameX.Core.Tests.Utility;

/// <summary>
/// 验证 <see cref="CryptogramUtility"/> 的密码哈希（Argon2id，Foundation.PasswordHashHelper）。
/// 覆盖正向 round-trip、错误密码、历史 MD5/SM2/SM4 密文拒绝、随机盐、null 边界。
/// </summary>
public class CryptogramUtilityPasswordHashTests
{
    public static IEnumerable<object[]> Passwords()
    {
        yield return new object[] { "123456", };
        yield return new object[] { "password", };
        yield return new object[] { "P@ssw0rd!#$%", };
        yield return new object[] { "管理员密码123", };
        yield return new object[] { new string('x', 128), };
    }

    [Theory]
    [MemberData(nameof(Passwords))]
    public void HashPassword_ReturnsArgon2idPhc(string password)
    {
        var hash = CryptogramUtility.HashPassword(password);

        Assert.StartsWith("$argon2id$", hash);
    }

    [Fact]
    public void HashPassword_UsesRandomSalt_DifferentHashEachCall()
    {
        var h1 = CryptogramUtility.HashPassword("same-password");
        var h2 = CryptogramUtility.HashPassword("same-password");

        Assert.NotEqual(h1, h2);
    }

    [Theory]
    [MemberData(nameof(Passwords))]
    public void VerifyPassword_CorrectPassword_ReturnsTrue(string password)
    {
        var hash = CryptogramUtility.HashPassword(password);

        Assert.True(CryptogramUtility.VerifyPassword(password, hash));
    }

    [Theory]
    [MemberData(nameof(Passwords))]
    public void VerifyPassword_WrongPassword_ReturnsFalse(string password)
    {
        var hash = CryptogramUtility.HashPassword(password);

        Assert.False(CryptogramUtility.VerifyPassword(password + "x", hash));
    }

    [Fact]
    public void VerifyPassword_InvalidStoredFormat_ReturnsFalse()
    {
        Assert.False(CryptogramUtility.VerifyPassword("123456", "not-a-valid-phc"));
    }

    [Fact]
    public void VerifyPassword_LegacyMd5Hex_ReturnsFalse()
    {
        // 历史 MD5 密文（32 hex）非 PHC 格式，必须拒绝
        Assert.False(CryptogramUtility.VerifyPassword("123456", "d41d8cd98f00b204e9800998ecf8427e"));
    }

    [Fact]
    public void VerifyPassword_LegacySm2Cipher_ReturnsFalse()
    {
        // 历史 SM2 密文（C1C2C3 大写 hex）非 PHC 格式，必须拒绝
        // ponytail: 2026-10-19 该样例串曾被脱敏成非法转义 "\*\*\*REMOVED\*\*\*" 使整个测试项目无法编译；
        // 此处为等价占位 hex（04 前缀 + C1C2C3 大写 hex 形态，非真实密文），仅取"非 PHC 格式"语义
        var sm2Like = "04A1B2C3D4E5F60718293A4B5C6D7E8F90112233445566778899AABBCCDDEEFF00112233445566778899AABBCCDDEEFF";
        Assert.False(CryptogramUtility.VerifyPassword("123456", sm2Like));
    }

    [Fact]
    public void HashPassword_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => CryptogramUtility.HashPassword(null!));
    }

    [Fact]
    public void VerifyPassword_NullInput_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => CryptogramUtility.VerifyPassword(null!, "$argon2id$fake"));
    }

    [Fact]
    public void VerifyPassword_NullStored_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => CryptogramUtility.VerifyPassword("123456", null!));
    }
}