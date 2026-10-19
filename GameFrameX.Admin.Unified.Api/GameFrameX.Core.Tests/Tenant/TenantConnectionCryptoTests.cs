using GameFrameX.Core.Service.Tenant;

namespace GameFrameX.Core.Tests.Tenant;

// C317（GFX-726）：租户库连接串加密收口 TenantConnectionCrypto 的行为契约。
// change：gfx-doc/dev/tenant/changes/C317-gfx-726-tenant-db-isolation-hardening/change.md
// 覆盖：往返还原、enc:v1: 前缀、明文兼容直通、幂等加密、空值透传、
// 未配置密钥的降级直存、密文 + 缺密钥 / 错误密钥 / 篡改密文的失败语义。
// 环境变量在本类内集中设置与还原（xunit 同类内串行，避免与其它测试竞争）。
public sealed class TenantConnectionCryptoTests : IDisposable
{
    private const string PlainConnection = "Host=127.0.0.1;Port=5432;Database=gameframex_admin_tenant_1;Username=tenant_user;Password=secret-password;";

    private readonly string? _originalKey;

    public TenantConnectionCryptoTests()
    {
        _originalKey = Environment.GetEnvironmentVariable(TenantConnectionCrypto.KeyEnvironmentVariable);
    }

    public void Dispose()
    {
        RestoreKey(_originalKey);
    }

    [Fact]
    public void EncryptThenDecrypt_RestoresOriginalConnection()
    {
        Environment.SetEnvironmentVariable(TenantConnectionCrypto.KeyEnvironmentVariable, "unit-test-encryption-key");

        var encrypted = TenantConnectionCrypto.Encrypt(PlainConnection);

        Assert.StartsWith(TenantConnectionCrypto.EncryptedPrefix, encrypted, StringComparison.Ordinal);
        Assert.NotEqual(PlainConnection, encrypted);
        Assert.DoesNotContain("secret-password", encrypted);
        Assert.Equal(PlainConnection, TenantConnectionCrypto.Decrypt(encrypted));
    }

    [Fact]
    public void Encrypt_EachCallProducesDifferentCipherText()
    {
        Environment.SetEnvironmentVariable(TenantConnectionCrypto.KeyEnvironmentVariable, "unit-test-encryption-key");

        var first = TenantConnectionCrypto.Encrypt(PlainConnection);
        var second = TenantConnectionCrypto.Encrypt(PlainConnection);

        // AesHelper 每次随机 Salt + Nonce，同明文两次密文必须不同（防彩虹对照）
        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Decrypt_PlaintextWithoutPrefix_PassesThrough()
    {
        Environment.SetEnvironmentVariable(TenantConnectionCrypto.KeyEnvironmentVariable, "unit-test-encryption-key");

        Assert.Equal(PlainConnection, TenantConnectionCrypto.Decrypt(PlainConnection));
    }

    [Fact]
    public void Encrypt_IsIdempotentForAlreadyEncryptedValue()
    {
        Environment.SetEnvironmentVariable(TenantConnectionCrypto.KeyEnvironmentVariable, "unit-test-encryption-key");
        var encrypted = TenantConnectionCrypto.Encrypt(PlainConnection);

        Assert.Equal(encrypted, TenantConnectionCrypto.Encrypt(encrypted));
    }

    [Fact]
    public void Encrypt_WithoutKey_ReturnsPlaintextDegradedMode()
    {
        Environment.SetEnvironmentVariable(TenantConnectionCrypto.KeyEnvironmentVariable, null);

        Assert.Equal(PlainConnection, TenantConnectionCrypto.Encrypt(PlainConnection));
    }

    [Fact]
    public void EncryptDecrypt_NullAndEmpty_PassThrough()
    {
        Environment.SetEnvironmentVariable(TenantConnectionCrypto.KeyEnvironmentVariable, "unit-test-encryption-key");

        Assert.Null(TenantConnectionCrypto.Encrypt(null));
        Assert.Null(TenantConnectionCrypto.Decrypt(null));
        Assert.Equal(string.Empty, TenantConnectionCrypto.Encrypt(string.Empty));
        Assert.Equal(string.Empty, TenantConnectionCrypto.Decrypt(string.Empty));
    }

    [Fact]
    public void Decrypt_EncryptedWithoutKey_ThrowsWithKeyHint()
    {
        Environment.SetEnvironmentVariable(TenantConnectionCrypto.KeyEnvironmentVariable, "unit-test-encryption-key");
        var encrypted = TenantConnectionCrypto.Encrypt(PlainConnection);
        Environment.SetEnvironmentVariable(TenantConnectionCrypto.KeyEnvironmentVariable, null);

        var exception = Assert.Throws<InvalidOperationException>(() => TenantConnectionCrypto.Decrypt(encrypted));
        Assert.Contains(TenantConnectionCrypto.KeyEnvironmentVariable, exception.Message);
    }

    [Fact]
    public void Decrypt_WithWrongKey_ThrowsWithoutLeakingCipherText()
    {
        Environment.SetEnvironmentVariable(TenantConnectionCrypto.KeyEnvironmentVariable, "unit-test-encryption-key");
        var encrypted = TenantConnectionCrypto.Encrypt(PlainConnection);
        Environment.SetEnvironmentVariable(TenantConnectionCrypto.KeyEnvironmentVariable, "another-encryption-key");

        var exception = Assert.Throws<InvalidOperationException>(() => TenantConnectionCrypto.Decrypt(encrypted));
        Assert.DoesNotContain(encrypted[TenantConnectionCrypto.EncryptedPrefix.Length..], exception.Message);
        Assert.DoesNotContain("secret-password", exception.Message);
    }

    [Fact]
    public void Decrypt_TamperedCipherText_Throws()
    {
        Environment.SetEnvironmentVariable(TenantConnectionCrypto.KeyEnvironmentVariable, "unit-test-encryption-key");
        var encrypted = TenantConnectionCrypto.Encrypt(PlainConnection);
        // 翻转密文尾部一个字符，模拟存储值被篡改（AES-GCM 认证标签校验必须失败）
        var tampered = encrypted[..^1] + (encrypted[^1] == 'A' ? 'B' : 'A');

        Assert.Throws<InvalidOperationException>(() => TenantConnectionCrypto.Decrypt(tampered));
    }

    [Fact]
    public void IsEncrypted_ClassifiesStoredValueForm()
    {
        Environment.SetEnvironmentVariable(TenantConnectionCrypto.KeyEnvironmentVariable, "unit-test-encryption-key");

        Assert.False(TenantConnectionCrypto.IsEncrypted(PlainConnection));
        Assert.True(TenantConnectionCrypto.IsEncrypted(TenantConnectionCrypto.Encrypt(PlainConnection)));
        Assert.False(TenantConnectionCrypto.IsEncrypted(null));
    }

    private static void RestoreKey(string? value)
    {
        if (value == null)
        {
            Environment.SetEnvironmentVariable(TenantConnectionCrypto.KeyEnvironmentVariable, null);
        }
        else
        {
            Environment.SetEnvironmentVariable(TenantConnectionCrypto.KeyEnvironmentVariable, value);
        }
    }
}
