using System.Text.Json.Nodes;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.Service.Tenant.ObjectStorage;

namespace GameFrameX.Core.Tests.Tenant;

// C370（GFX-829）：对象存储 ConfigJson schema 驱动字段级加解密收口 TenantObjectStorageConfigCrypto 的行为契约。
// change：gfx-doc/dev/system-management/changes/C370-tenant-secret-crypto-foundation/change.md
// 用真实 aliyun schema（AccessKeySecret 唯一敏感字段，Endpoint/BucketName/AccessKeyId 明文）验证：
// 字段级选择性加密、往返还原、幂等、降级直存、fail-closed、畸形 JSON 直通、无敏感字段 Provider 直通、随机密文各自可解。
public sealed class TenantObjectStorageConfigCryptoTests : IDisposable
{
    private const string PlainJson = "{\"Endpoint\":\"oss-cn-hangzhou.aliyuncs.com\",\"BucketName\":\"game-bucket\",\"AccessKeyId\":\"LTAI5tExample\",\"AccessKeySecret\":\"AKSecretExample2026\",\"PublicBaseUrl\":\"https://cdn.example.com\"}";

    private readonly string? _originalSecretKey;

    public TenantObjectStorageConfigCryptoTests()
    {
        _originalSecretKey = Environment.GetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable);
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, _originalSecretKey);
    }

    [Fact]
    public void EncryptConfigJson_EncryptsOnlySensitiveFields()
    {
        var encryptedJson = TenantObjectStorageConfigCrypto.EncryptConfigJson("aliyun", PlainJson);

        var node = JsonNode.Parse(encryptedJson)!.AsObject();
        Assert.Equal("oss-cn-hangzhou.aliyuncs.com", (string?)node["Endpoint"]);
        Assert.Equal("game-bucket", (string?)node["BucketName"]);
        Assert.Equal("LTAI5tExample", (string?)node["AccessKeyId"]);
        Assert.Equal("https://cdn.example.com", (string?)node["PublicBaseUrl"]);
        var secret = (string?)node["AccessKeySecret"];
        Assert.StartsWith(TenantSecretCrypto.EncryptedPrefix, secret, StringComparison.Ordinal);
        Assert.DoesNotContain("AKSecretExample", secret);
    }

    [Fact]
    public void DecryptConfigJson_RestoresOriginalJson()
    {
        var encryptedJson = TenantObjectStorageConfigCrypto.EncryptConfigJson("aliyun", PlainJson);

        var decryptedJson = TenantObjectStorageConfigCrypto.DecryptConfigJson("aliyun", encryptedJson);

        var node = JsonNode.Parse(decryptedJson)!.AsObject();
        Assert.Equal("oss-cn-hangzhou.aliyuncs.com", (string?)node["Endpoint"]);
        Assert.Equal("game-bucket", (string?)node["BucketName"]);
        Assert.Equal("LTAI5tExample", (string?)node["AccessKeyId"]);
        Assert.Equal("AKSecretExample2026", (string?)node["AccessKeySecret"]);
        Assert.Equal("https://cdn.example.com", (string?)node["PublicBaseUrl"]);
    }

    [Fact]
    public void EncryptConfigJson_IsIdempotentForAlreadyEncryptedJson()
    {
        var encryptedJson = TenantObjectStorageConfigCrypto.EncryptConfigJson("aliyun", PlainJson);

        // 敏感字段已是密文 → 幂等跳过，整串 JSON 逐字节不变（重写场景不换密文）
        Assert.Equal(encryptedJson, TenantObjectStorageConfigCrypto.EncryptConfigJson("aliyun", encryptedJson));
    }

    [Fact]
    public void EncryptConfigJson_WithoutKey_ReturnsPlaintextDegradedMode()
    {
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, null);

        Assert.Equal(PlainJson, TenantObjectStorageConfigCrypto.EncryptConfigJson("aliyun", PlainJson));
    }

    [Fact]
    public void DecryptConfigJson_EncryptedWithoutKey_ThrowsFailClosed()
    {
        var encryptedJson = TenantObjectStorageConfigCrypto.EncryptConfigJson("aliyun", PlainJson);
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, null);

        var exception = Assert.Throws<InvalidOperationException>(() => TenantObjectStorageConfigCrypto.DecryptConfigJson("aliyun", encryptedJson));
        Assert.Contains(TenantSecretCrypto.KeyEnvironmentVariable, exception.Message);
    }

    [Fact]
    public void Transform_MalformedJson_PassesThrough()
    {
        // 与 ObjectStorageConfigNormalizer.ParseObject 同款容错：畸形 JSON 不在收口职责内，原样返回交上层校验
        Assert.Equal("not-a-json", TenantObjectStorageConfigCrypto.EncryptConfigJson("aliyun", "not-a-json"));
        Assert.Equal("not-a-json", TenantObjectStorageConfigCrypto.DecryptConfigJson("aliyun", "not-a-json"));
    }

    [Fact]
    public void Transform_LocalProviderWithoutSensitiveFields_PassesThrough()
    {
        // local provider 无 Sensitive 字段：ConfigJson 原样直通（含正常 JSON）
        var localJson = "{\"RootPath\":\"/data/upload\"}";
        Assert.Equal(localJson, TenantObjectStorageConfigCrypto.EncryptConfigJson("local", localJson));
        Assert.Equal(localJson, TenantObjectStorageConfigCrypto.DecryptConfigJson("local", localJson));
    }

    [Fact]
    public void DecryptConfigJson_EachRandomCipherText_DecryptsCorrectly()
    {
        // 每次加密随机 Salt + Nonce 产生不同密文，解密缓存按密文原文为 key——新密文必须重新解密且结果正确
        var first = TenantObjectStorageConfigCrypto.EncryptConfigJson("aliyun", PlainJson);
        var second = TenantObjectStorageConfigCrypto.EncryptConfigJson("aliyun", PlainJson);
        Assert.NotEqual(first, second);

        foreach (var cipher in new[] { first, second, })
        {
            var node = JsonNode.Parse(TenantObjectStorageConfigCrypto.DecryptConfigJson("aliyun", cipher))!.AsObject();
            Assert.Equal("AKSecretExample2026", (string?)node["AccessKeySecret"]);
        }
    }
}
