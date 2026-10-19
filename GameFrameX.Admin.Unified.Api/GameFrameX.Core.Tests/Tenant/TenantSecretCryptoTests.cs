using GameFrameX.Core.Service.Tenant;

namespace GameFrameX.Core.Tests.Tenant;

// C370（GFX-829）：业务凭证静态加密收口 TenantSecretCrypto 的行为契约。
// change：gfx-doc/dev/system-management/changes/C370-tenant-secret-crypto-foundation/change.md
// 覆盖：往返还原、enc:sec:v1: 前缀、明文兼容直通、幂等加密、空值透传、
// 未配置密钥的降级直存、密文 + 缺密钥 / 错误密钥 / 篡改密文的失败语义、
// 与连接串域 enc:v1: 密文互不认领（双向）。
// 测试程序集已 DisableTestParallelization，环境变量在本类内集中设置与还原。
public sealed class TenantSecretCryptoTests : IDisposable
{
    private const string PlainSecret = "AKIDexample-secret-key-2026";

    private readonly string? _originalSecretKey;
    private readonly string? _originalConnectionKey;

    public TenantSecretCryptoTests()
    {
        _originalSecretKey = Environment.GetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable);
        _originalConnectionKey = Environment.GetEnvironmentVariable(TenantConnectionCrypto.KeyEnvironmentVariable);
    }

    public void Dispose()
    {
        Restore(TenantSecretCrypto.KeyEnvironmentVariable, _originalSecretKey);
        Restore(TenantConnectionCrypto.KeyEnvironmentVariable, _originalConnectionKey);
    }

    [Fact]
    public void EncryptThenDecrypt_RestoresOriginalSecret()
    {
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");

        var encrypted = TenantSecretCrypto.Encrypt(PlainSecret);

        Assert.StartsWith(TenantSecretCrypto.EncryptedPrefix, encrypted, StringComparison.Ordinal);
        Assert.NotEqual(PlainSecret, encrypted);
        Assert.DoesNotContain("example-secret-key", encrypted);
        Assert.Equal(PlainSecret, TenantSecretCrypto.Decrypt(encrypted));
    }

    [Fact]
    public void Encrypt_EachCallProducesDifferentCipherText()
    {
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");

        // AesHelper 每次随机 Salt + Nonce，同明文两次密文必须不同（防彩虹对照）
        Assert.NotEqual(TenantSecretCrypto.Encrypt(PlainSecret), TenantSecretCrypto.Encrypt(PlainSecret));
    }

    [Fact]
    public void Decrypt_PlaintextWithoutPrefix_PassesThrough()
    {
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");

        Assert.Equal(PlainSecret, TenantSecretCrypto.Decrypt(PlainSecret));
    }

    [Fact]
    public void Encrypt_IsIdempotentForAlreadyEncryptedValue()
    {
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");
        var encrypted = TenantSecretCrypto.Encrypt(PlainSecret);

        Assert.Equal(encrypted, TenantSecretCrypto.Encrypt(encrypted));
    }

    [Fact]
    public void EncryptDecrypt_NullAndEmpty_PassThrough()
    {
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");

        Assert.Null(TenantSecretCrypto.Encrypt(null));
        Assert.Null(TenantSecretCrypto.Decrypt(null));
        Assert.Equal(string.Empty, TenantSecretCrypto.Encrypt(string.Empty));
        Assert.Equal(string.Empty, TenantSecretCrypto.Decrypt(string.Empty));
    }

    [Fact]
    public void Encrypt_WithoutKey_ReturnsPlaintextDegradedMode()
    {
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, null);

        Assert.Equal(PlainSecret, TenantSecretCrypto.Encrypt(PlainSecret));
    }

    [Fact]
    public void Decrypt_EncryptedWithoutKey_ThrowsWithKeyHint()
    {
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");
        var encrypted = TenantSecretCrypto.Encrypt(PlainSecret);
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, null);

        var exception = Assert.Throws<InvalidOperationException>(() => TenantSecretCrypto.Decrypt(encrypted));
        Assert.Contains(TenantSecretCrypto.KeyEnvironmentVariable, exception.Message);
    }

    [Fact]
    public void Decrypt_WithWrongKey_ThrowsWithoutLeakingCipherText()
    {
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");
        var encrypted = TenantSecretCrypto.Encrypt(PlainSecret);
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, "another-secret-key");

        var exception = Assert.Throws<InvalidOperationException>(() => TenantSecretCrypto.Decrypt(encrypted));
        Assert.DoesNotContain(encrypted[TenantSecretCrypto.EncryptedPrefix.Length..], exception.Message);
        Assert.DoesNotContain("example-secret-key", exception.Message);
    }

    [Fact]
    public void Decrypt_TamperedCipherText_Throws()
    {
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");
        var encrypted = TenantSecretCrypto.Encrypt(PlainSecret);
        // 翻转密文尾部一个字符，模拟存储值被篡改（AES-GCM 认证标签校验必须失败）
        var tampered = encrypted[..^1] + (encrypted[^1] == 'A' ? 'B' : 'A');

        Assert.Throws<InvalidOperationException>(() => TenantSecretCrypto.Decrypt(tampered));
    }

    [Fact]
    public void IsEncrypted_ClassifiesStoredValueForm()
    {
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");

        Assert.False(TenantSecretCrypto.IsEncrypted(PlainSecret));
        Assert.True(TenantSecretCrypto.IsEncrypted(TenantSecretCrypto.Encrypt(PlainSecret)));
        Assert.False(TenantSecretCrypto.IsEncrypted(null));
    }

    [Fact]
    public void Decrypt_ConnectionCipherText_PassesThroughWithoutClaiming()
    {
        // 两域前缀互斥（enc:sec:v1: vs enc:v1:）：连接串密文对凭证收口是「无前缀值」，直通不认领、不误抛解密失败
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");
        Environment.SetEnvironmentVariable(TenantConnectionCrypto.KeyEnvironmentVariable, "unit-test-connection-key");
        var connectionCipher = TenantConnectionCrypto.Encrypt("Host=127.0.0.1;Password=conn-pwd");

        Assert.False(TenantSecretCrypto.IsEncrypted(connectionCipher));
        Assert.Equal(connectionCipher, TenantSecretCrypto.Decrypt(connectionCipher));
    }

    [Fact]
    public void ConnectionCrypto_DoesNotClaimSecretCipherText()
    {
        // 反向互不认领：凭证密文对连接串收口同样是无前缀值——防止两类密文跨收口互解误用
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");
        Environment.SetEnvironmentVariable(TenantConnectionCrypto.KeyEnvironmentVariable, "unit-test-connection-key");
        var secretCipher = TenantSecretCrypto.Encrypt(PlainSecret);

        Assert.False(TenantConnectionCrypto.IsEncrypted(secretCipher));
        Assert.Equal(secretCipher, TenantConnectionCrypto.Decrypt(secretCipher));
    }

    private static void Restore(string variable, string? value)
    {
        Environment.SetEnvironmentVariable(variable, value);
    }
}
