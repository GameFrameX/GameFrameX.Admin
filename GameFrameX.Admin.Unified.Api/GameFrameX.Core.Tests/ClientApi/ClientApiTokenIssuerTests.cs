using GameFrameX.Client.Api.Services.Auth;
using GameFrameX.Client.Api.Services.GameLoginTicket;
using System.Security.Cryptography;
using System.Text.Json;
using GameFrameX.Client.Api.GameRole.Dto;
using GameFrameX.Client.Api.Services;
using GameFrameX.Client.Api.Services.ClientApiTenantOptions;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;
using Microsoft.IdentityModel.JsonWebTokens;

namespace GameFrameX.Core.Tests.ClientApi;

public class ClientApiTokenIssuerTests
{
    [Fact]
    public async Task AccessToken_ValidatesUserSubject()
    {
        var provider = FakeOptionsProvider.WithTokenPolicy("unit-test-signing-key", 60);
        var issuer = new ClientApiTokenIssuer(provider);
        var user = new TenantGameUserEntity
        {
            Id = 1001,
            Status = GameUserStatus.Active,
        };

        var token = await issuer.IssueAsync(user, "Email");
        var validation = await issuer.ValidateAsync(token.Token);

        Assert.True(validation.IsValid);
        Assert.Equal(user.Id, validation.UserId);
        Assert.True(token.ExpireTime > DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }

    [Fact]
    public async Task AccessToken_MissingSigningKey_RejectsValidation()
    {
        // 租户未配置 SigningKey 时，禁止回落到进程级或全局密钥，任何令牌都校验失败
        var provider = FakeOptionsProvider.WithoutTokenPolicy();
        var issuer = new ClientApiTokenIssuer(provider);

        var validation = await issuer.ValidateAsync("any.token.value");

        Assert.False(validation.IsValid);
    }

    [Fact]
    public async Task GameLoginTicket_ContainsRequiredClaims()
    {
        var provider = FakeOptionsProvider.WithTicketPolicy(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60);
        var keyResolver = TestGameLoginTicketKeyResolver.Create();
        var ticketIssuer = new GameLoginTicketIssuer(keyResolver, provider);

        var ticket = await ticketIssuer.IssueAsync(new GameLoginTicketIssueContext(10, 20, 30, 40, 50, "wss://game.example.test:9443"));
        var jwt = new JsonWebToken(ticket.Ticket);

        // issuer / audience 固定为平台默认，不读取租户配置
        Assert.Equal(ClientApiPlatformDefaults.GameLoginTicketIssuer, jwt.Issuer);
        Assert.Equal(ClientApiPlatformDefaults.GameLoginTicketAudience, jwt.Audiences.Single());
        Assert.Equal(keyResolver.Kid, jwt.Kid);
        Assert.Equal(keyResolver.Kid, ticket.Kid);
        Assert.Equal("10", jwt.Claims.Single(c => c.Type == "tenantId").Value);
        Assert.Equal("20", jwt.Claims.Single(c => c.Type == "appId").Value);
        Assert.Equal("30", jwt.Claims.Single(c => c.Type == "userId").Value);
        Assert.Equal("40", jwt.Claims.Single(c => c.Type == "playerId").Value);
        Assert.Equal("50", jwt.Claims.Single(c => c.Type == "areaId").Value);
        Assert.False(string.IsNullOrWhiteSpace(jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Jti).Value));
    }

    [Fact]
    public async Task GameLoginTicket_ExpireTimeReadsPolicyValue()
    {
        // 票据 exp 直接取自租户票据策略的固定到期时间（Unix 秒）
        var expireAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 120;
        var provider = FakeOptionsProvider.WithTicketPolicy(expireAt);
        var keyResolver = TestGameLoginTicketKeyResolver.Create();
        var ticketIssuer = new GameLoginTicketIssuer(keyResolver, provider);

        var ticket = await ticketIssuer.IssueAsync(new GameLoginTicketIssueContext(10, 20, 30, 40, 50, "wss://game.example.test:9443"));
        var jwt = new JsonWebToken(ticket.Ticket);

        var exp = long.Parse(jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Exp).Value);
        Assert.Equal(expireAt, exp);
        Assert.Equal(expireAt, ticket.ExpireTime);
    }

    [Fact]
    public async Task GameLoginTicket_ZeroExpireTime_OmitsExpClaim()
    {
        // TicketExpireTime 为 0 时票据永不过期：JWT 不含 exp claim，结果 ExpireTime 为 null。
        var provider = FakeOptionsProvider.WithTicketPolicy(0);
        var keyResolver = TestGameLoginTicketKeyResolver.Create();
        var ticketIssuer = new GameLoginTicketIssuer(keyResolver, provider);

        var ticket = await ticketIssuer.IssueAsync(new GameLoginTicketIssueContext(10, 20, 30, 40, 50, "wss://game.example.test:9443"));
        var jwt = new JsonWebToken(ticket.Ticket);

        Assert.Null(ticket.ExpireTime);
        Assert.DoesNotContain(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Exp);
    }

    [Fact]
    public async Task GameLoginTicket_ExpireTimeOverThirtyDays_IsClamped()
    {
        var before = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var configuredExpireAt = before + ClientApiPlatformDefaults.GameLoginTicketLifetimeSecondsMax + 3600;
        var provider = FakeOptionsProvider.WithTicketPolicy(configuredExpireAt);
        var keyResolver = TestGameLoginTicketKeyResolver.Create();
        var ticketIssuer = new GameLoginTicketIssuer(keyResolver, provider);

        var ticket = await ticketIssuer.IssueAsync(new GameLoginTicketIssueContext(10, 20, 30, 40, 50, "wss://game.example.test:9443"));
        var after = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var jwt = new JsonWebToken(ticket.Ticket);
        var exp = long.Parse(jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Exp).Value);

        Assert.NotNull(ticket.ExpireTime);
        Assert.Equal(ticket.ExpireTime.Value, exp);
        Assert.InRange(exp, before + ClientApiPlatformDefaults.GameLoginTicketLifetimeSecondsMax, after + ClientApiPlatformDefaults.GameLoginTicketLifetimeSecondsMax);
    }

    [Theory]
    [InlineData(0, 1440)]
    [InlineData(30, 30)]
    [InlineData(20000, 10080)]
    public void ClientApiTokenLifetime_ClampHonorsPlatformRange(int configured, int expected)
    {
        // <=0 使用平台默认 1440；区间内原值；超过上限取上限 10080
        Assert.Equal(expected, ClientApiPlatformDefaults.ClampTokenLifetimeMinutes(configured));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(999, 999)]
    [InlineData(2595600, 2592000)]
    public void GameLoginTicketExpireTime_NormalizeHonorsPlatformRange(long offsetSeconds, long expectedOffsetSeconds)
    {
        const long now = 100000;
        var configuredExpireTime = offsetSeconds == 0 ? 0 : now + offsetSeconds;
        var expectedExpireTime = expectedOffsetSeconds == 0 ? 0 : now + expectedOffsetSeconds;

        Assert.Equal(expectedExpireTime, ClientApiPlatformDefaults.NormalizeGameLoginTicketExpireTime(configuredExpireTime, now));
    }

    [Fact]
    public void GamePlayerLoginInput_CapturesForbiddenContextFields()
    {
        const string payload = "{"
                               + "\"PlayerId\":40,"
                               + "\"AreaId\":50,"
                               + "\"AccessToken\":\"token\","
                               + "\"UserId\":30,"
                               + "\"tenantId\":10,"
                               + "\"appId\":20"
                               + "}";

        var input = JsonSerializer.Deserialize<GamePlayerLoginRequest>(payload);

        Assert.NotNull(input);
        Assert.Contains(input!.ExtraFields.Keys, key => string.Equals(key, "UserId", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(input.ExtraFields.Keys, key => string.Equals(key, "tenantId", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(input.ExtraFields.Keys, key => string.Equals(key, "appId", StringComparison.OrdinalIgnoreCase));
    }

    private sealed class TestGameLoginTicketKeyResolver : IGameLoginTicketKeyResolver
    {
        private readonly GameLoginTicketSigningKey _key;

        private TestGameLoginTicketKeyResolver(GameLoginTicketSigningKey key, string kid)
        {
            _key = key;
            Kid = kid;
        }

        public string Kid { get; }

        public static TestGameLoginTicketKeyResolver Create()
        {
            using (var rsa = RSA.Create(2048))
            {
                var kid = "tenant-10-app-20-test";
                var privateKeyPem = rsa.ExportPkcs8PrivateKeyPem();
                var publicKeyPem = rsa.ExportSubjectPublicKeyInfoPem();
                return new TestGameLoginTicketKeyResolver(GameLoginTicketSigningKey.CreateRsa(kid, privateKeyPem, publicKeyPem), kid);
            }
        }

        public Task<GameLoginTicketSigningKey> ResolveSigningKeyAsync(long tenantId, long appId)
        {
            Assert.Equal(10, tenantId);
            Assert.Equal(20, appId);
            return Task.FromResult(_key);
        }

        public Task<IReadOnlyCollection<GameLoginTicketPublicKey>> GetPublicKeysAsync(long tenantId, long appId)
        {
            IReadOnlyCollection<GameLoginTicketPublicKey> keys = new[]
            {
                new GameLoginTicketPublicKey(Kid, GameLoginTicketSigningKey.AlgorithmRs256, _key.PublicKeyPem, null, null),
            };
            return Task.FromResult(keys);
        }
    }
}