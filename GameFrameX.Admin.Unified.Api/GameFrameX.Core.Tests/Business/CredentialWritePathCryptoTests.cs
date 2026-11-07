using System.Text.Json.Nodes;
using GameFrameX.Advertisement.Abstract;
using GameFrameX.Application.ClientDelivery.AdvertisementCallbackConfig;
using GameFrameX.Application.ClientDelivery.AdvertisementCallbackConfig.Dto;
using GameFrameX.Application.ClientDelivery.Security.AppSignatureKey;
using GameFrameX.Application.ClientDelivery.Security.AppSignatureKey.Dto;
using GameFrameX.Core.Service;
using GameFrameX.Core.Service.Business;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Business.ClientApiConfig;
using GameFrameX.Entity.Business.Game;
using GameFrameX.Entity.Business.Game.App;
using Microsoft.Extensions.Logging.Abstractions;

namespace GameFrameX.Core.Tests.Business;

// ：凭证写入路径「确定最终明文之后、落库之前加密」两条代表链路。
// change：gfx-doc/dev/game-business/changes/-credential-encryption-application/change.md
// 密钥环（SignatureKey）：Add/Rotate 响应一次性回显明文，库内密文且可解密还原；
// JSON 凭证（Advertisement）：Add 后汇总列密文 + ConfigJson 敏感字段密文、非敏感字段明文，Update 掩码保留。
// 其余实体（SessionKey / TicketKey / Policy / OpenAccess / Payment）与上述同型插入，
// 由 CredentialWritePathGuardTests 源码级守护覆盖。
public sealed class CredentialWritePathCryptoTests : IDisposable
{
    private readonly ClientApiTestDb _db = new();
    private readonly string? _originalKey;

    public CredentialWritePathCryptoTests()
    {
        _originalKey = Environment.GetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable);
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");
        _db.InitBusinessTables(typeof(AppSignatureKeyEntity), typeof(AdvertisementCallbackConfigEntity), typeof(GameAppPackageEntity));
        _db.SeedPackage(8801, "com.example.ad");
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, _originalKey);
        _db.Dispose();
    }

    [Fact]
    public async Task SignatureKey_AddAndRotate_RevealPlaintextOnce_StoreCipherOnly()
    {
        var service = new AppSignatureKeyService(
            new Repository<AppSignatureKeyEntity>(_db.MainDb));

        var added = await service.AddAsync(new AppSignatureKeyAddRequest { Remark = "unit-add", });
        var storedAdded = _db.MainDb.Queryable<AppSignatureKeyEntity>().Filter(null, true).First(u => u.Id == added.Id);
        Assert.StartsWith(SecretCrypto.EncryptedPrefix, storedAdded.Secret, StringComparison.Ordinal);
        Assert.Equal(added.Secret, SecretCrypto.Decrypt(storedAdded.Secret));

        var rotated = await service.RotateAsync(new AppSignatureKeyRotateRequest { });
        var storedRotated = _db.MainDb.Queryable<AppSignatureKeyEntity>().Filter(null, true).First(u => u.Id == rotated.Id);
        Assert.StartsWith(SecretCrypto.EncryptedPrefix, storedRotated.Secret, StringComparison.Ordinal);
        Assert.Equal(rotated.Secret, SecretCrypto.Decrypt(storedRotated.Secret));
        Assert.NotEqual(storedAdded.Secret, storedRotated.Secret);
    }


    [Fact]
    public async Task AdvertisementConfig_AddEncryptsColumnsAndSensitiveJsonFields_MaskUpdatePreserves()
    {
        var service = new AdvertisementCallbackConfigService(
            new Repository<AdvertisementCallbackConfigEntity>(_db.MainDb),
            new AdvertisementCallbackConfigSchemaProvider(),
            new Repository<GameAppPackageEntity>(_db.MainDb));
        var configJson = "{\"callbackUrl\":\"https://cb.example.com/hook\",\"signingSecret\":\"ad-plain-secret\"}";

        var added = await service.AddAsync(new AddAdvertisementCallbackConfigRequest
        {
            PackageId = 8801,
            Provider = "Pangle",
            CallbackUrl = "https://cb.example.com/hook",
            SigningSecret = "ad-plain-secret",
            ConfigJson = configJson,
        });

        var stored = _db.MainDb.Queryable<AdvertisementCallbackConfigEntity>().Filter(null, true).First(u => u.Id == added.Id);
        Assert.StartsWith(SecretCrypto.EncryptedPrefix, stored.SigningSecret, StringComparison.Ordinal);
        Assert.Equal("ad-plain-secret", SecretCrypto.Decrypt(stored.SigningSecret));

        // 字段级：callbackUrl 明文可读，signingSecret 密文可还原
        var json = JsonNode.Parse(stored.ConfigJson)!.AsObject();
        Assert.Equal("https://cb.example.com/hook", (string?)json["callbackUrl"]);
        Assert.Equal("ad-plain-secret", SecretCrypto.Decrypt((string?)json["signingSecret"]));

        // 回传 ******：NormalizeConfigJson 用库内密文回填掩码字段，EncryptConfigJson 幂等保留原密文。
        // 汇总列在掩码更新时重定基到 JSON 携带的密文（列与 JSON 各自独立加盐，本就不是同一密文串）——
        // 断言语义保留：仍是密文、解出同一明文、且与 JSON 字段密文一致。
        var cipherJsonBefore = stored.ConfigJson;
        await service.UpdateAsync(new UpdateAdvertisementCallbackConfigRequest
        {
            Id = added.Id,
            PackageId = 8801,
            Provider = "Pangle",
            CallbackUrl = "https://cb.example.com/hook",
            SigningSecret = SensitiveFieldMask.MaskValue,
            VerificationKeys = SensitiveFieldMask.MaskValue,
            ConfigJson = "{\"callbackUrl\":\"https://cb.example.com/hook\",\"signingSecret\":\"******\"}",
            RowVersion = stored.RowVersion,
        });
        stored = _db.MainDb.Queryable<AdvertisementCallbackConfigEntity>().Filter(null, true).First(u => u.Id == added.Id);
        Assert.Equal(cipherJsonBefore, stored.ConfigJson);
        Assert.StartsWith(SecretCrypto.EncryptedPrefix, stored.SigningSecret, StringComparison.Ordinal);
        Assert.Equal("ad-plain-secret", SecretCrypto.Decrypt(stored.SigningSecret));
        Assert.Equal((string?)JsonNode.Parse(stored.ConfigJson)!["signingSecret"], stored.SigningSecret);
    }
}
