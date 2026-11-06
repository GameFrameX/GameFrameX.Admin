using System.Reflection;
using System.Security.Cryptography;
using GameFrameX.Advertisement.Abstract;
using GameFrameX.Client.Api.Integrations.Payment;
using GameFrameX.Client.Api.Services.ClientApiOptions;
using GameFrameX.Client.Api.Services.GameLoginTicket;
using GameFrameX.Core.Middleware;
using GameFrameX.Core.Service.Business;
using GameFrameX.Entity.Business.ClientApiConfig;
using GameFrameX.Entity.Business.Game;
using GameFrameX.Payment.AliPay;
using Microsoft.AspNetCore.Http;

namespace GameFrameX.Core.Tests.ClientApi;

// C372（GFX-838）T2/T5/T6：运行时凭证读取解密收口——混合库（明文行 + 密文行共存）下
// Client.Api 六链路全部还原明文可用；密文存在而密钥缺失 fail-closed。
// change：gfx-doc/dev/client-api/changes/C372-runtime-credential-decrypt-closure/change.md
// 链路清单（T1 notes.md）：签名密钥环 / 会话密钥环 / 票据 RSA 私钥 / 选项 Provider（邮件·广告·令牌策略）/ 支付配置。
// Hub 链路（开放访问中间件）在 GameFrameX.Core.Tests/HubApi/GameAppOpenAccessDecryptClosureTests.cs。
public sealed class RuntimeCredentialDecryptClosureTests : IDisposable
{
    private readonly ClientApiTestDb _db = new();
    private readonly string? _originalKey;

    public RuntimeCredentialDecryptClosureTests()
    {
        _originalKey = Environment.GetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable);
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");

        // 支付渠道注册表反射扫描已加载程序集——显式加载渠道程序集，保证 alipay 渠道 schema 可解析
        _ = Assembly.Load("GameFrameX.Payment.AliPay");
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, _originalKey);
        _db.Dispose();
    }

    [Fact]
    public async Task SignatureKeyResolver_MixedRows_ResolvesPlaintextForBoth()
    {
        _db.InitBusinessTables(typeof(AppSignatureKeyEntity));
        _db.MainDb.Insertable(new AppSignatureKeyEntity
        {
            Id = 7101, Kid = "kid-plain", Secret = "sig-plain", IsEnabled = true,
        }).ExecuteCommand();
        _db.MainDb.Insertable(new AppSignatureKeyEntity
        {
            Id = 7102, Kid = "kid-cipher", Secret = SecretCrypto.Encrypt("sig-cipher")!, IsEnabled = true,
        }).ExecuteCommand();

        var resolver = new SignatureKeyResolver(_db.DbProvider);

        var plainLookup = await resolver.ResolveAsync("kid-plain");
        var cipherLookup = await resolver.ResolveAsync("kid-cipher");

        Assert.Equal("sig-plain", plainLookup.Secret);
        Assert.Equal("sig-cipher", cipherLookup.Secret);

        var activeKeys = await resolver.GetActiveKeysAsync();
        Assert.Equal(new[] { "sig-cipher", "sig-plain" }, activeKeys.Select(k => k.Secret).OrderBy(s => s, StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public async Task SessionKeyResolver_MixedRows_ResolvesPlaintextForBoth()
    {
        _db.InitBusinessTables(typeof(SessionKeyEntity));
        _db.MainDb.Insertable(new SessionKeyEntity
        {
            Id = 7201, Kid = "kid-plain", Secret = "session-plain", IsEnabled = true,
        }).ExecuteCommand();
        _db.MainDb.Insertable(new SessionKeyEntity
        {
            Id = 7202, Kid = "kid-cipher", Secret = SecretCrypto.Encrypt("session-cipher")!, IsEnabled = true,
        }).ExecuteCommand();

        var resolver = new SessionKeyResolver(_db.DbProvider);

        var plainLookup = await resolver.ResolveAsync("kid-plain");
        var cipherLookup = await resolver.ResolveAsync("kid-cipher");

        Assert.Equal("session-plain", plainLookup.Secret);
        Assert.Equal("session-cipher", cipherLookup.Secret);
    }

    [Fact]
    public async Task LoginTicketKeyResolver_EncryptedPrivateKey_ResolvesUsableRsaKey()
    {
        _db.InitBusinessTables(typeof(GameLoginTicketKeyEntity));
        using (var rsa = RSA.Create(2048))
        {
            _db.MainDb.Insertable(new GameLoginTicketKeyEntity
            {
                Id = 7301, Kid = "kid-ticket",
                Algorithm = GameLoginTicketSigningKey.AlgorithmRs256,
                PrivateKeyPem = SecretCrypto.Encrypt(rsa.ExportPkcs8PrivateKeyPem())!,
                PublicKeyPem = rsa.ExportSubjectPublicKeyInfoPem(),
                IsEnabled = true,
            }).ExecuteCommand();
        }

        // 存在生效 key 时 optionsProvider（自动建 key 开关）不被触达，传 null 即可
        var resolver = new GameLoginTicketKeyResolver(_db.DbProvider, null!);

        var signingKey = await resolver.ResolveSigningKeyAsync();

        // CreateRsa 内部 ImportFromPem——解密失败（密文当 PEM）必然抛错，返回即证明解密链路通
        Assert.Equal("kid-ticket", signingKey.Kid);
    }

    [Fact]
    public async Task OptionsProvider_TokenPolicy_SigningKeyDecrypts()
    {
        _db.InitBusinessTables(typeof(ClientApiPolicyEntity));
        _db.MainDb.Insertable(new ClientApiPolicyEntity
        {
            Id = 7401, SigningKey = SecretCrypto.Encrypt("policy-signing-key")!, IsEnabled = true,
        }).ExecuteCommand();

        var provider = new ClientApiOptionsProvider(_db.DbProvider, new HttpContextAccessor { HttpContext = _db.CreateHttpContext(), });

        var policy = await provider.GetTokenPolicyAsync();

        Assert.Equal("policy-signing-key", policy.SigningKey);
    }

    [Fact]
    public async Task OptionsProvider_Advertisement_JsonSensitiveFieldDecrypts()
    {
        _db.InitBusinessTables(typeof(AdvertisementCallbackConfigEntity));
        var configJson = AdvertisementCallbackConfigCrypto.EncryptConfigJson("Pangle",
            "{\"callbackUrl\":\"https://cb.example.com/hook\",\"signingSecret\":\"ad-json-secret\"}");
        _db.MainDb.Insertable(new AdvertisementCallbackConfigEntity
        {
            Id = 7501, Provider = "Pangle",
            SigningSecret = SecretCrypto.Encrypt("ad-column-secret")!, VerificationKeys = string.Empty,
            ConfigJson = configJson, IsEnabled = true,
        }).ExecuteCommand();

        var provider = new ClientApiOptionsProvider(_db.DbProvider, new HttpContextAccessor { HttpContext = _db.CreateHttpContext(), });

        var options = await provider.GetAdvertisementCallbackOptionsAsync();

        // JSON 内 signingSecret 为敏感键 → 字段级解密生效（ConfigJson 是验签材料唯一来源，汇总列不再回退读取）
        Assert.Equal("ad-json-secret", options.SigningSecret);
        Assert.Equal("ad-json-secret", options.PangleSecurityKey);
        Assert.Equal("https://cb.example.com/hook", options.ConfigValues["callbackUrl"]);
        Assert.Contains("ad-json-secret", options.ConfigJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OptionsProvider_Advertisement_MissingRequiredSecretKey_Throws()
    {
        _db.InitBusinessTables(typeof(AdvertisementCallbackConfigEntity));
        // ConfigJson 是验签材料唯一来源：Pangle 缺 signingSecret 直接 fail-closed，不再回落汇总列
        _db.MainDb.Insertable(new AdvertisementCallbackConfigEntity
        {
            Id = 7511, Provider = "Pangle",
            SigningSecret = SecretCrypto.Encrypt("ad-column-secret")!, VerificationKeys = string.Empty,
            ConfigJson = "{\"callbackUrl\":\"https://cb.example.com/hook\"}", IsEnabled = true,
        }).ExecuteCommand();

        var provider = new ClientApiOptionsProvider(_db.DbProvider, new HttpContextAccessor { HttpContext = _db.CreateHttpContext(), });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetAdvertisementCallbackOptionsAsync());
        Assert.Contains("signingSecret", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OptionsProvider_Advertisement_AdMobRequiresVerificationKeys()
    {
        _db.InitBusinessTables(typeof(AdvertisementCallbackConfigEntity));
        // AdMob 行必填 verificationKeys；signingSecret 缺失不影响（AdMob 用 verificationKeys 验签）
        _db.MainDb.Insertable(new AdvertisementCallbackConfigEntity
        {
            Id = 7512, Provider = "GoogleAdMob",
            ConfigJson = "{\"callbackUrl\":\"https://cb.example.com/admob\",\"verificationKeys\":\"k1=base64key\"}", IsEnabled = true,
        }).ExecuteCommand();

        var provider = new ClientApiOptionsProvider(_db.DbProvider, new HttpContextAccessor { HttpContext = _db.CreateHttpContext(), });

        var options = await provider.GetAdvertisementCallbackOptionsAsync();

        Assert.Equal("k1=base64key", options.VerificationKeys);
        Assert.Single(options.AdMobKeys);
        Assert.True(options.AdMobKeys.ContainsKey("k1"));
    }

    [Fact]
    public async Task OptionsProvider_Email_SecretIdKeyDecrypts()
    {
        _db.InitBusinessTables(typeof(EmailConfigEntity));
        _db.MainDb.Insertable(new EmailConfigEntity
        {
            Id = 7601, FromEmailAddress = "noreply@example.com",
            FromEmailSecretId = SecretCrypto.Encrypt("email-plain-id")!,
            FromEmailSecretKey = SecretCrypto.Encrypt("email-plain-key")!,
            IsEnabled = true,
        }).ExecuteCommand();

        var provider = new ClientApiOptionsProvider(_db.DbProvider, new HttpContextAccessor { HttpContext = _db.CreateHttpContext(), });

        var options = await provider.GetEmailOptionsAsync();

        Assert.Equal("email-plain-id", options.FromEmailSecretId);
        Assert.Equal("email-plain-key", options.FromEmailSecretKey);
    }

    [Fact]
    public void PaymentProvider_Sqlite_MainDatabaseQuery_ReturnsDecryptedOption()
    {
        // 回归锁定：作用域 Where 的 Nullable.HasValue 闭包曾使 SqlSugar 生成非法 SQL
        // （SQLite 报 near "IS"、PG 报 near "NOT"），已提为表达式树外局部布尔修复
        _db.InitBusinessTables(typeof(GamePaymentConfigEntity));
        var configJson = PaymentConfigCrypto.EncryptConfigJson("alipay",
            "{\"AliPayAppId\":\"2021001\",\"AliPayMerchantPrivateKey\":\"plain-merchant-key\",\"AliPayGatewayUrl\":\"https://openapi.alipay.com/gateway.do\"}");
        _db.MainDb.Insertable(new GamePaymentConfigEntity
        {
            Id = 7701, PayChannel = "alipay", ConfigJson = configJson, IsEnabled = true,
        }).ExecuteCommand();

        var context = _db.CreateHttpContext(include: false, includeApp: false);
        context.Items[GameFrameX.Core.Middleware.PublicCallbackRouteContext.ItemKey] = true;
        var provider = new PaymentOptionSettingProvider(_db.DbProvider, new HttpContextAccessor { HttpContext = context, });

        var setting = provider.GetOptionSetting<AliPayOptionSetting>("alipay");

        Assert.NotNull(setting);
        Assert.Equal("plain-merchant-key", setting.AliPayMerchantPrivateKey);
    }

    [Fact]
    public void TenantSecretCrypto_FailClosed_CiphertextWithoutKey_ThrowsWithEnvVarHint()
    {
        var cipher = SecretCrypto.Encrypt("fail-closed-secret")!;
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, null);

        var exception = Assert.Throws<InvalidOperationException>(() => SecretCrypto.Decrypt(cipher));
        Assert.Contains(SecretCrypto.KeyEnvironmentVariable, exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(cipher, exception.Message, StringComparison.Ordinal);

        // DecryptCached 同语义：fail-closed 抛出不落缓存，重复调用同样抛
        Assert.Throws<InvalidOperationException>(() => SecretCrypto.DecryptCached(cipher));
        Assert.Throws<InvalidOperationException>(() => SecretCrypto.DecryptCached(cipher));
    }

    [Fact]
    public void TenantSecretCrypto_DecryptCached_PlaintextPassthrough_AndStableResult()
    {
        var cipher = SecretCrypto.Encrypt("cached-secret")!;

        Assert.Equal("plain-value", SecretCrypto.DecryptCached("plain-value"));
        Assert.Equal("cached-secret", SecretCrypto.DecryptCached(cipher));
        Assert.Equal("cached-secret", SecretCrypto.DecryptCached(cipher));
    }
}
