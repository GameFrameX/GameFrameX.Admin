using System.Reflection;
using System.Text.RegularExpressions;
using GameFrameX.Application.Game.App.CredentialCenter;
using GameFrameX.Application.Game.App.CredentialCenter.Dto;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Tenant.ClientApiConfig;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Entity.Tenant.Game.App;
using GameFrameX.Entity.Tenant.ObjectStorage;

namespace GameFrameX.Core.Tests.Tenant;

// C374（GFX-838）：凭证中心聚合只读 API。
// change：gfx-doc/dev/game-business/changes/C374-credential-center-aggregate-api/change.md
// 四类卡片聚合正确性（密钥可用口径 = 启用+已生效+未到期，RotatedTime 纯元数据不排除）、
// 零密钥材料不变量（反射扫描 Response 图：字符串属性名不得命中敏感词、任何属性不得为 byte[]）、
// 应用归属校验（复用 C194 ownership validator，查不到即拒绝）。
public sealed class TenantAppCredentialOverviewTests : IDisposable
{
    private readonly ClientApiTestDb _db = new();

    public TenantAppCredentialOverviewTests()
    {
        _db.SeedApp();
        _db.InitTenantTables(
            typeof(TenantAppSignatureKeyEntity),
            typeof(TenantGameAppOpenAccessEntity),
            typeof(TenantGameLoginTicketKeyEntity),
            typeof(TenantClientApiPolicyEntity),
            typeof(TenantSessionKeyEntity),
            typeof(TenantGamePaymentConfigEntity),
            typeof(TenantEmailConfigEntity),
            typeof(TenantSmsConfigEntity),
            typeof(TenantWechatConfigEntity),
            typeof(TenantAdvertisementCallbackConfigEntity),
            typeof(TenantObjectStorageConfigEntity));
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    private TenantAppCredentialOverviewService CreateService()
    {
        return new TenantAppCredentialOverviewService(
            _db.Repository<TenantGameAppEntity>(),
            _db.Repository<TenantAppSignatureKeyEntity>(),
            _db.Repository<TenantGameAppOpenAccessEntity>(),
            _db.Repository<TenantGameLoginTicketKeyEntity>(),
            _db.Repository<TenantClientApiPolicyEntity>(),
            _db.Repository<TenantSessionKeyEntity>(),
            _db.Repository<TenantGamePaymentConfigEntity>(),
            _db.Repository<TenantEmailConfigEntity>(),
            _db.Repository<TenantSmsConfigEntity>(),
            _db.Repository<TenantWechatConfigEntity>(),
            _db.Repository<TenantAdvertisementCallbackConfigEntity>(),
            _db.Repository<TenantObjectStorageConfigEntity>());
    }

    /// <summary>SeedFullCredentialSet 写入的基准时间（窗口断言用）。</summary>
    private long _seedNow;

    /// <summary>SeedFullCredentialSet 写入的最近到期时间（HMAC 生效钥 ExpireTime）。</summary>
    private long _seedNearExpiry;

    /// <summary>预置含全类凭证的应用数据（时间基准落 _seedNow/_seedNearExpiry 供窗口断言）。</summary>
    private void SeedFullCredentialSet()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var nearExpiry = now + 500;
        _seedNow = now;
        _seedNearExpiry = nearExpiry;

        // 应用身份：AppSecretHash 已配置 + 已重置过 + 强制签名
        var app = _db.TenantDb.Queryable<TenantGameAppEntity>().First(u => u.Id == _db.AppId);
        app.AppSecretHash = "pbkdf2$test$hash";
        app.LastRotatedTime = now - 3600;
        app.SignatureRequired = true;
        _db.TenantDb.Updateable(app).ExecuteCommand();

        // 传输签名：HMAC 环 2 行——生效 / 已过期（过期行启用态保留，隔离连接级 IsEnabled 过滤，专测到期谓词）；
        // OpenAccess 2 行——活跃 / 已吊销（吊销行启用态保留，专测 RevokedTime 谓词）
        _db.TenantDb.Insertable(new TenantAppSignatureKeyEntity
        {
            AppId = _db.AppId, Kid = "hmac-active", Secret = "enc:sec:v1:a", NotBeforeTime = now - 100, ExpireTime = nearExpiry, IsEnabled = true, TenantId = _db.TenantId,
        }).ExecuteCommand();
        _db.TenantDb.Insertable(new TenantAppSignatureKeyEntity
        {
            AppId = _db.AppId, Kid = "hmac-expired", Secret = "enc:sec:v1:b", NotBeforeTime = now - 200, ExpireTime = now - 10, IsEnabled = true, TenantId = _db.TenantId,
        }).ExecuteCommand();
        _db.TenantDb.Insertable(new TenantGameAppOpenAccessEntity
        {
            AppId = _db.AppId, AccessKey = "ak-active", AccessSecret = "enc:sec:v1:d", Scenario = "test", Purpose = "test",
            SubjectName = "sub", ScopesJson = "[]", ExpiresTime = null, RevokedTime = null, IsEnabled = true, UpdateTime = now - 50, TenantId = _db.TenantId,
        }).ExecuteCommand();
        _db.TenantDb.Insertable(new TenantGameAppOpenAccessEntity
        {
            AppId = _db.AppId, AccessKey = "ak-revoked", AccessSecret = "enc:sec:v1:e", Scenario = "test", Purpose = "test",
            SubjectName = "sub", ScopesJson = "[]", ExpiresTime = null, RevokedTime = now - 1, IsEnabled = true, UpdateTime = now - 60, TenantId = _db.TenantId,
        }).ExecuteCommand();

        // 票据与令牌：票据 RSA 1 把（永不过期）；应用行策略（应用行优先于默认行）；会话密钥 2 把
        _db.TenantDb.Insertable(new TenantGameLoginTicketKeyEntity
        {
            AppId = _db.AppId, Kid = "ticket-1", PrivateKeyPem = "enc:sec:v1:p", PublicKeyPem = "pub", NotBeforeTime = now - 100, ExpireTime = null, IsEnabled = true, TenantId = _db.TenantId,
        }).ExecuteCommand();
        _db.TenantDb.Insertable(new TenantClientApiPolicyEntity
        {
            AppId = _db.AppId, SigningKey = "enc:sec:v1:k", LifetimeMinutes = 45, EnableKeyAutoCreate = true, IsEnabled = true, TenantId = _db.TenantId,
        }).ExecuteCommand();
        _db.TenantDb.Insertable(new TenantClientApiPolicyEntity
        {
            AppId = 0, SigningKey = "enc:sec:v1:k0", LifetimeMinutes = 30, EnableKeyAutoCreate = false, IsEnabled = true, TenantId = _db.TenantId,
        }).ExecuteCommand();
        _db.TenantDb.Insertable(new TenantSessionKeyEntity
        {
            AppId = _db.AppId, Kid = "session-1", Secret = "enc:sec:v1:s1", NotBeforeTime = now - 100, ExpireTime = null, IsEnabled = true, TenantId = _db.TenantId,
        }).ExecuteCommand();
        _db.TenantDb.Insertable(new TenantSessionKeyEntity
        {
            AppId = _db.AppId, Kid = "session-2", Secret = "enc:sec:v1:s2", NotBeforeTime = now - 100, ExpireTime = null, IsEnabled = true, TenantId = _db.TenantId,
        }).ExecuteCommand();

        // 出站对接：支付（全局行 + 应用行）；邮件（应用行 + 默认行）；SMS（仅默认行）；微信（无）；广告回调（应用行）；对象存储（租户行）
        _db.TenantDb.Insertable(new TenantGamePaymentConfigEntity
        {
            AppId = 0, GameAppIdNormalized = 0, PackageIdNormalized = 0, PayChannel = "google", IsEnabled = true, TenantId = _db.TenantId,
        }).ExecuteCommand();
        _db.TenantDb.Insertable(new TenantGamePaymentConfigEntity
        {
            AppId = _db.AppId, GameAppIdNormalized = _db.AppId, PackageIdNormalized = 0, PayChannel = "apple", IsEnabled = true, TenantId = _db.TenantId,
        }).ExecuteCommand();
        _db.TenantDb.Insertable(new TenantEmailConfigEntity
        {
            AppId = _db.AppId, Purpose = 1, FromEmailAddress = "app@example.test", FromEmailSecretId = "id", FromEmailSecretKey = "enc:sec:v1:m", Priority = 50, IsEnabled = true, TenantId = _db.TenantId,
        }).ExecuteCommand();
        _db.TenantDb.Insertable(new TenantEmailConfigEntity
        {
            AppId = 0, Purpose = 1, FromEmailAddress = "default@example.test", FromEmailSecretId = "id0", FromEmailSecretKey = "enc:sec:v1:m0", Priority = 10, IsEnabled = true, TenantId = _db.TenantId,
        }).ExecuteCommand();
        _db.TenantDb.Insertable(new TenantSmsConfigEntity
        {
            AppId = 0, AccessKeyId = "sms-id", AccessKeySecret = "enc:sec:v1:sms", SignName = "sign", Priority = 100, IsEnabled = true, TenantId = _db.TenantId,
        }).ExecuteCommand();
        _db.TenantDb.Insertable(new TenantAdvertisementCallbackConfigEntity
        {
            AppId = _db.AppId, CallbackUrl = "https://cb.example.test", SigningSecret = "enc:sec:v1:cb", IsEnabled = true, TenantId = _db.TenantId,
        }).ExecuteCommand();
        _db.TenantDb.Insertable(new TenantObjectStorageConfigEntity
        {
            Provider = "s3", ConfigJson = "enc:sec:v1:os", IsEnabled = true, TenantId = _db.TenantId,
        }).ExecuteCommand();
    }

    [Fact]
    public async Task Overview_AggregatesFourCards_Correctly()
    {
        SeedFullCredentialSet();
        var now = _seedNow;
        var nearExpiry = _seedNearExpiry;

        var response = await CreateService().GetAsync(_db.AppId);

        // ① 应用身份
        Assert.Equal(_db.AppId, response.AppId);
        Assert.Equal("C108 Test App", response.AppName);
        Assert.True(response.Identity.AppSecretConfigured);
        Assert.InRange(response.Identity.AppSecretLastRotatedTime!.Value, now - 3600, now);
        Assert.True(response.Identity.SignatureRequired);

        // ② 传输签名：可用口径排除已过期/已禁用；最新 kid 取 NotBefore 最大；OpenAccess 排除已吊销
        Assert.Equal(1, response.Transport.HmacActiveKeyCount);
        Assert.Equal("hmac-active", response.Transport.LatestHmacKid);
        Assert.Equal(nearExpiry, response.Transport.HmacNearestExpireTime);
        Assert.Equal(1, response.Transport.OpenAccessActiveCount);
        Assert.InRange(response.Transport.OpenAccessLastRotatedTime!.Value, now - 60, now - 50);

        // ③ 票据与令牌：应用行策略优先；票据永不过期 → nearest 为 null；会话密钥 2 把
        Assert.Equal(1, response.TicketToken.TicketKeyActiveCount);
        Assert.Null(response.TicketToken.TicketKeyNearestExpireTime);
        Assert.True(response.TicketToken.TicketKeyAutoCreateEnabled);
        Assert.Equal(45, response.TicketToken.TokenLifetimeMinutes);
        Assert.True(response.TicketToken.TokenSigningKeyConfigured);
        Assert.Equal(TenantAppCredentialOverviewResponse.ScopeApp, response.TicketToken.PolicyScope);
        Assert.Equal(2, response.TicketToken.SessionKeyActiveCount);

        // ④ 出站对接：作用域命中（应用行优先）+ 启用态 + Priority + 配置页路由
        var outbound = response.Outbound.ToDictionary(u => u.Kind);
        Assert.Equal(6, outbound.Count);
        Assert.Equal(TenantAppCredentialOverviewResponse.ScopeApp, outbound["payment"].Scope);
        Assert.Equal(2, outbound["payment"].ConfiguredCount);
        Assert.True(outbound["payment"].Enabled); // 全局行启用、应用行禁用 → 任一命中行启用即 true
        Assert.Equal(TenantAppCredentialOverviewResponse.ScopeApp, outbound["email"].Scope);
        Assert.Equal(1, outbound["email"].ConfiguredCount);
        Assert.Equal(50, outbound["email"].Priority);
        Assert.Equal(TenantAppCredentialOverviewResponse.ScopeTenantDefault, outbound["sms"].Scope);
        Assert.Equal(100, outbound["sms"].Priority);
        Assert.Equal(TenantAppCredentialOverviewResponse.ScopeNone, outbound["wechat"].Scope);
        Assert.Equal(0, outbound["wechat"].ConfiguredCount);
        Assert.Equal(TenantAppCredentialOverviewResponse.ScopeApp, outbound["adCallback"].Scope);
        Assert.Null(outbound["adCallback"].Priority); // 广告回调实体无 Priority 字段
        Assert.Equal(TenantAppCredentialOverviewResponse.ScopeTenant, outbound["objectStorage"].Scope);
        Assert.Equal("/app/appCenter/gamePaymentConfigEntity", outbound["payment"].RoutePath);
    }

    [Fact]
    public async Task Overview_PolicyFallsBackToTenantDefault_WhenAppRowMissing()
    {
        _db.TenantDb.Insertable(new TenantClientApiPolicyEntity
        {
            AppId = 0, SigningKey = "enc:sec:v1:k0", LifetimeMinutes = 30, EnableKeyAutoCreate = true, IsEnabled = true, TenantId = _db.TenantId,
        }).ExecuteCommand();

        var response = await CreateService().GetAsync(_db.AppId);

        Assert.Equal(TenantAppCredentialOverviewResponse.ScopeTenantDefault, response.TicketToken.PolicyScope);
        Assert.Equal(30, response.TicketToken.TokenLifetimeMinutes);
        Assert.True(response.TicketToken.TicketKeyAutoCreateEnabled);
        Assert.Equal(0, response.TicketToken.TicketKeyActiveCount);
        Assert.Equal(0, response.TicketToken.SessionKeyActiveCount);
    }

    [Fact]
    public async Task Overview_NoPolicy_ScopeNoneAndDefaults()
    {
        var response = await CreateService().GetAsync(_db.AppId);

        Assert.Equal(TenantAppCredentialOverviewResponse.ScopeNone, response.TicketToken.PolicyScope);
        Assert.Equal(0, response.TicketToken.TokenLifetimeMinutes);
        Assert.False(response.TicketToken.TokenSigningKeyConfigured);
        Assert.False(response.TicketToken.TicketKeyAutoCreateEnabled);
    }

    [Fact]
    public async Task Overview_AppIdZero_Rejected()
    {
        await Assert.ThrowsAsync<BusinessException>(() => CreateService().GetAsync(0));
    }

    [Fact]
    public async Task Overview_AppNotInTenant_Rejected()
    {
        await Assert.ThrowsAsync<BusinessException>(() => CreateService().GetAsync(41060401));
    }

    /// <summary>
    /// 零密钥材料不变量（C374 §2）：递归扫描 Response 全图——字符串属性名不得命中敏感词
    /// （bool/int/long? 的「已配置/计数/时间戳」字段不属于密钥材料，允许带 Secret/Token 等词根），
    /// 任何属性不得为 byte[]。后续给卡片加字段即被本测试约束。
    /// </summary>
    [Fact]
    public void Overview_ResponseGraph_ContainsNoSecretMaterial()
    {
        var sensitiveNamePattern = new Regex("secret|private|cipher|pem|password|key|token|sign", RegexOptions.IgnoreCase);
        var visited = new HashSet<Type>();
        AssertNoSecretMaterial(typeof(TenantAppCredentialOverviewResponse), sensitiveNamePattern, visited);
    }

    private static void AssertNoSecretMaterial(Type type, Regex sensitiveNamePattern, HashSet<Type> visited)
    {
        if (!visited.Add(type))
        {
            return;
        }

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            Assert.NotEqual(typeof(byte[]), property.PropertyType);

            if (property.PropertyType == typeof(string) && sensitiveNamePattern.IsMatch(property.Name))
            {
                Assert.Fail($"Response 图发现敏感命名字符串属性：{type.Name}.{property.Name}（零密钥材料不变量被破坏）");
            }

            var propertyType = property.PropertyType;
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                AssertNoSecretMaterial(propertyType.GetGenericArguments()[0], sensitiveNamePattern, visited);
            }
            else if (propertyType.IsClass && propertyType != typeof(string))
            {
                AssertNoSecretMaterial(propertyType, sensitiveNamePattern, visited);
            }
        }
    }

    /// <summary>用途矩阵常量与 U14 §1 四类分类法一致（单一事实来源）：4 类 12 成员、成员唯一。</summary>
    [Fact]
    public void UsageMatrix_MatchesU14FourCategories()
    {
        var matrix = TenantAppCredentialOverviewResponse.BuildUsageMatrix();

        Assert.Equal(12, matrix.Count);
        var categories = matrix.Select(u => u.Category).Distinct().ToList();
        Assert.Equal(new[] { "identity", "outbound", "ticketToken", "transport" }, categories.OrderBy(u => u, StringComparer.Ordinal).ToList());
        Assert.Equal(1, matrix.Count(u => u.Category == "identity"));
        Assert.Equal(2, matrix.Count(u => u.Category == "transport"));
        Assert.Equal(3, matrix.Count(u => u.Category == "ticketToken"));
        Assert.Equal(6, matrix.Count(u => u.Category == "outbound"));
        Assert.Equal(matrix.Count, matrix.Select(u => u.Member).Distinct().Count()); // 成员唯一
        Assert.All(matrix, u =>
        {
            Assert.False(string.IsNullOrWhiteSpace(u.Consumers));
            Assert.False(string.IsNullOrWhiteSpace(u.Operations));
            Assert.False(string.IsNullOrWhiteSpace(u.AlgorithmForm));
        });
    }
}
