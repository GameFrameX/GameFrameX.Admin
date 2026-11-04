using System.Text.Json.Nodes;
using GameFrameX.Advertisement.Abstract;
using GameFrameX.Application.ClientDelivery.AdvertisementCallbackConfig;
using GameFrameX.Application.ClientDelivery.AdvertisementCallbackConfig.Dto;
using GameFrameX.Application.ClientDelivery.Security.AppSignatureKey;
using GameFrameX.Application.ClientDelivery.Security.AppSignatureKey.Dto;
using GameFrameX.Core.Service;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Tenant.ClientApiConfig;
using GameFrameX.Entity.Tenant.Game;
using Microsoft.Extensions.Logging.Abstractions;

namespace GameFrameX.Core.Tests.Tenant;

// C371（GFX-838）T6：凭证写入路径「确定最终明文之后、落库之前加密」两条代表链路。
// change：gfx-doc/dev/game-business/changes/C371-tenant-credential-encryption-application/change.md
// 密钥环（SignatureKey）：Add/Rotate 响应一次性回显明文，库内密文且可解密还原；
// JSON 凭证（Advertisement）：Add 后汇总列密文 + ConfigJson 敏感字段密文、非敏感字段明文，Update 掩码保留。
// 其余实体（SessionKey / TicketKey / Policy / OpenAccess / Payment）与上述同型插入，
// 由 TenantCredentialWritePathGuardTests 源码级守护覆盖。
public sealed class TenantCredentialWritePathCryptoTests : IDisposable
{
    private readonly ClientApiTestDb _db = new();
    private readonly string? _originalKey;

    public TenantCredentialWritePathCryptoTests()
    {
        _originalKey = Environment.GetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable);
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");
        _db.InitTenantTables(typeof(TenantAppSignatureKeyEntity), typeof(TenantAdvertisementCallbackConfigEntity));
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, _originalKey);
        _db.Dispose();
    }

    [Fact]
    public async Task SignatureKey_AddAndRotate_RevealPlaintextOnce_StoreCipherOnly()
    {
        var service = new AppSignatureKeyService(
            new Repository<TenantAppSignatureKeyEntity>(_db.MainDb));

        var added = await service.AddAsync(new AppSignatureKeyAddRequest { Remark = "unit-add", });
        var storedAdded = _db.TenantDb.Queryable<TenantAppSignatureKeyEntity>().Filter(null, true).First(u => u.Id == added.Id);
        Assert.StartsWith(TenantSecretCrypto.EncryptedPrefix, storedAdded.Secret, StringComparison.Ordinal);
        Assert.Equal(added.Secret, TenantSecretCrypto.Decrypt(storedAdded.Secret));

        var rotated = await service.RotateAsync(new AppSignatureKeyRotateRequest { });
        var storedRotated = _db.TenantDb.Queryable<TenantAppSignatureKeyEntity>().Filter(null, true).First(u => u.Id == rotated.Id);
        Assert.StartsWith(TenantSecretCrypto.EncryptedPrefix, storedRotated.Secret, StringComparison.Ordinal);
        Assert.Equal(rotated.Secret, TenantSecretCrypto.Decrypt(storedRotated.Secret));
        Assert.NotEqual(storedAdded.Secret, storedRotated.Secret);
    }


    [Fact]
    public async Task AdvertisementConfig_AddEncryptsColumnsAndSensitiveJsonFields_MaskUpdatePreserves()
    {
        var service = new AdvertisementCallbackConfigService(
            new Repository<TenantAdvertisementCallbackConfigEntity>(_db.MainDb),
            new AdvertisementCallbackConfigSchemaProvider());
        var configJson = "{\"callbackUrl\":\"https://cb.example.com/hook\",\"signingSecret\":\"ad-plain-secret\"}";

        var added = await service.AddAsync(new AddTenantAdvertisementCallbackConfigRequest
        {
           
            Provider = "Pangle",
            CallbackUrl = "https://cb.example.com/hook",
            SigningSecret = "ad-plain-secret",
            ConfigJson = configJson,
        });

        var stored = _db.TenantDb.Queryable<TenantAdvertisementCallbackConfigEntity>().Filter(null, true).First(u => u.Id == added.Id);
        Assert.StartsWith(TenantSecretCrypto.EncryptedPrefix, stored.SigningSecret, StringComparison.Ordinal);
        Assert.Equal("ad-plain-secret", TenantSecretCrypto.Decrypt(stored.SigningSecret));

        // 字段级：callbackUrl 明文可读，signingSecret 密文可还原
        var json = JsonNode.Parse(stored.ConfigJson)!.AsObject();
        Assert.Equal("https://cb.example.com/hook", (string?)json["callbackUrl"]);
        Assert.Equal("ad-plain-secret", TenantSecretCrypto.Decrypt((string?)json["signingSecret"]));

        // 回传 ******：NormalizeConfigJson 用库内密文回填掩码字段，EncryptConfigJson 幂等保留原密文。
        // 汇总列在掩码更新时重定基到 JSON 携带的密文（列与 JSON 各自独立加盐，本就不是同一密文串）——
        // 断言语义保留：仍是密文、解出同一明文、且与 JSON 字段密文一致。
        var cipherJsonBefore = stored.ConfigJson;
        await service.UpdateAsync(new UpdateTenantAdvertisementCallbackConfigRequest
        {
            Id = added.Id,
           
            Provider = "Pangle",
            CallbackUrl = "https://cb.example.com/hook",
            SigningSecret = SensitiveFieldMask.MaskValue,
            VerificationKeys = SensitiveFieldMask.MaskValue,
            ConfigJson = "{\"callbackUrl\":\"https://cb.example.com/hook\",\"signingSecret\":\"******\"}",
            RowVersion = stored.RowVersion,
        });
        stored = _db.TenantDb.Queryable<TenantAdvertisementCallbackConfigEntity>().Filter(null, true).First(u => u.Id == added.Id);
        Assert.Equal(cipherJsonBefore, stored.ConfigJson);
        Assert.StartsWith(TenantSecretCrypto.EncryptedPrefix, stored.SigningSecret, StringComparison.Ordinal);
        Assert.Equal("ad-plain-secret", TenantSecretCrypto.Decrypt(stored.SigningSecret));
        Assert.Equal((string?)JsonNode.Parse(stored.ConfigJson)!["signingSecret"], stored.SigningSecret);
    }
}
