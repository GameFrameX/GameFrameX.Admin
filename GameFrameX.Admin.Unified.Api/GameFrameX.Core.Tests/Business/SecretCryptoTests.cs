using GameFrameX.Core.Service.Business;

namespace GameFrameX.Core.Tests.Business;

// C370（GFX-829）：业务凭证静态加密收口 SecretCrypto 的行为契约。
// change：gfx-doc/dev/system-management/changes/C370-secret-crypto-foundation/change.md
// 覆盖：往返还原、enc:sec:v1: 前缀、明文兼容直通、幂等加密、空值透传、
// 未配置密钥的降级直存、密文 + 缺密钥 / 错误密钥 / 篡改密文的失败语义、
// 与连接串域 enc:v1: 密文互不认领（双向）。
// 测试程序集已 DisableTestParallelization，环境变量在本类内集中设置与还原。
public sealed class SecretCryptoTests : IDisposable
{
    private const string PlainSecret = "AKIDexample-secret-key-2026";

    private readonly string? _originalSecretKey;

    public SecretCryptoTests()
    {
        _originalSecretKey = Environment.GetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable);
    }

    public void Dispose()
    {
        Restore(SecretCrypto.KeyEnvironmentVariable, _originalSecretKey);
    }

    [Fact]
    public void EncryptThenDecrypt_RestoresOriginalSecret()
    {
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");

        var encrypted = SecretCrypto.Encrypt(PlainSecret);

        Assert.StartsWith(SecretCrypto.EncryptedPrefix, encrypted, StringComparison.Ordinal);
        Assert.NotEqual(PlainSecret, encrypted);
        Assert.DoesNotContain("example-secret-key", encrypted);
        Assert.Equal(PlainSecret, SecretCrypto.Decrypt(encrypted));
    }

    [Fact]
    public void Encrypt_EachCallProducesDifferentCipherText()
    {
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");

        // AesHelper 每次随机 Salt + Nonce，同明文两次密文必须不同（防彩虹对照）
        Assert.NotEqual(SecretCrypto.Encrypt(PlainSecret), SecretCrypto.Encrypt(PlainSecret));
    }

    [Fact]
    public void Decrypt_PlaintextWithoutPrefix_PassesThrough()
    {
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");

        Assert.Equal(PlainSecret, SecretCrypto.Decrypt(PlainSecret));
    }

    [Fact]
    public void Encrypt_IsIdempotentForAlreadyEncryptedValue()
    {
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");
        var encrypted = SecretCrypto.Encrypt(PlainSecret);

        Assert.Equal(encrypted, SecretCrypto.Encrypt(encrypted));
    }

    [Fact]
    public void EncryptDecrypt_NullAndEmpty_PassThrough()
    {
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");

        Assert.Null(SecretCrypto.Encrypt(null));
        Assert.Null(SecretCrypto.Decrypt(null));
        Assert.Equal(string.Empty, SecretCrypto.Encrypt(string.Empty));
        Assert.Equal(string.Empty, SecretCrypto.Decrypt(string.Empty));
    }

    [Fact]
    public void Encrypt_WithoutKey_ReturnsPlaintextDegradedMode()
    {
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, null);

        Assert.Equal(PlainSecret, SecretCrypto.Encrypt(PlainSecret));
    }

    [Fact]
    public void Decrypt_EncryptedWithoutKey_ThrowsWithKeyHint()
    {
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");
        var encrypted = SecretCrypto.Encrypt(PlainSecret);
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, null);

        var exception = Assert.Throws<InvalidOperationException>(() => SecretCrypto.Decrypt(encrypted));
        Assert.Contains(SecretCrypto.KeyEnvironmentVariable, exception.Message);
    }

    [Fact]
    public void Decrypt_WithWrongKey_ThrowsWithoutLeakingCipherText()
    {
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");
        var encrypted = SecretCrypto.Encrypt(PlainSecret);
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, "another-secret-key");

        var exception = Assert.Throws<InvalidOperationException>(() => SecretCrypto.Decrypt(encrypted));
        Assert.DoesNotContain(encrypted[SecretCrypto.EncryptedPrefix.Length..], exception.Message);
        Assert.DoesNotContain("example-secret-key", exception.Message);
    }

    [Fact]
    public void Decrypt_TamperedCipherText_Throws()
    {
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");
        var encrypted = SecretCrypto.Encrypt(PlainSecret);
        // 翻转密文尾部一个字符，模拟存储值被篡改（AES-GCM 认证标签校验必须失败）
        var tampered = encrypted[..^1] + (encrypted[^1] == 'A' ? 'B' : 'A');

        Assert.Throws<InvalidOperationException>(() => SecretCrypto.Decrypt(tampered));
    }

    [Fact]
    public void IsEncrypted_ClassifiesStoredValueForm()
    {
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");

        Assert.False(SecretCrypto.IsEncrypted(PlainSecret));
        Assert.True(SecretCrypto.IsEncrypted(SecretCrypto.Encrypt(PlainSecret)));
        Assert.False(SecretCrypto.IsEncrypted(null));
    }

    [Fact]
    public void Decrypt_ConnectionCipherText_PassesThroughWithoutClaiming()
    {
        // 两域前缀互斥（enc:sec:v1: vs enc:v1:）：连接串密文对凭证收口是「无前缀值」，直通不认领、不误抛解密失败
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");
        const string connectionCipher = "enc:v1:legacy-connection-cipher";

        Assert.False(SecretCrypto.IsEncrypted(connectionCipher));
        Assert.Equal(connectionCipher, SecretCrypto.Decrypt(connectionCipher));
    }

    private static void Restore(string variable, string? value)
    {
        Environment.SetEnvironmentVariable(variable, value);
    }
}
