using System.Security.Cryptography;
using GameFrameX.Client.Api.Services.ClientApiTenantOptions;
using GameFrameX.Client.Api.Services.GameLoginTicket;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Core.Service.Tenant;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace GameFrameX.Core.Tests.ClientApi;

public sealed class GameLoginTicketSingleMainTests : IDisposable
{
    private readonly string? _originalKey = Environment.GetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable);

    public GameLoginTicketSingleMainTests()
    {
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, "ticket-single-main-test-secret");
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, _originalKey);
    }

    [Fact]
    public async Task IssuedTicket_VerifiesWithPublishedMainDatabasePublicKey()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitTenantTables(typeof(TenantGameLoginTicketKeyEntity));
            var options = FakeOptionsProvider.WithTicketPolicy(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 120);
            var resolver = new GameLoginTicketKeyResolver(database.MainDb, options);
            var issuer = new GameLoginTicketIssuer(resolver, options);
            var ticket = await issuer.IssueAsync(new GameLoginTicketIssueContext(1, 2, 3, "wss://game.example.test"));
            var publishedKey = Assert.Single(await resolver.GetPublicKeysAsync());
            Assert.Equal(ticket.Kid, publishedKey.Kid);
            Assert.True(Guid.TryParseExact(ticket.Kid, "N", out _));
            var storedKey = await database.MainDb.Queryable<TenantGameLoginTicketKeyEntity>().FirstAsync();
            Assert.DoesNotContain("PRIVATE KEY", storedKey.PrivateKeyPem);
            using (var rsa = RSA.Create())
            {
                rsa.ImportFromPem(publishedKey.PublicKeyPem);
                var validation = await new JsonWebTokenHandler().ValidateTokenAsync(ticket.Ticket, new TokenValidationParameters
                {
                    ValidIssuer = ClientApiPlatformDefaults.GameLoginTicketIssuer,
                    ValidAudience = ClientApiPlatformDefaults.GameLoginTicketAudience,
                    IssuerSigningKey = new RsaSecurityKey(rsa.ExportParameters(false)) { KeyId = publishedKey.Kid },
                    ValidateLifetime = true,
                });
                Assert.True(validation.IsValid, validation.Exception?.ToString());
            }
        }
    }

    [Fact]
    public async Task SigningAndPublishing_ShareLeaseAndAlgorithmFilters_AndPreserveExistingKid()
    {
        using (var database = new ClientApiTestDb())
        using (var rsa = RSA.Create(2048))
        {
            database.InitTenantTables(typeof(TenantGameLoginTicketKeyEntity));
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var entities = new[]
            {
                new TenantGameLoginTicketKeyEntity { Id = 1, Kid = "tenant-10-app-20-existing", IsEnabled = true },
                new TenantGameLoginTicketKeyEntity { Id = 2, Kid = "disabled", IsEnabled = false },
                new TenantGameLoginTicketKeyEntity { Id = 3, Kid = "expired", IsEnabled = true, ExpireTime = now - 1 },
                new TenantGameLoginTicketKeyEntity { Id = 4, Kid = "future", IsEnabled = true, NotBeforeTime = now + 300 },
                new TenantGameLoginTicketKeyEntity { Id = 5, Kid = "wrong-algorithm", IsEnabled = true },
            };
            foreach (var entity in entities)
            {
                entity.Algorithm = entity.Id == 5 ? "HS256" : GameLoginTicketSigningKey.AlgorithmRs256;
                entity.PrivateKeyPem = rsa.ExportPkcs8PrivateKeyPem();
                entity.PublicKeyPem = rsa.ExportSubjectPublicKeyInfoPem();
            }

            await database.MainDb.Insertable(entities).ExecuteCommandAsync();
            var resolver = new GameLoginTicketKeyResolver(database.MainDb, FakeOptionsProvider.WithTicketPolicy(now + 60));
            var signing = await resolver.ResolveSigningKeyAsync();
            var published = Assert.Single(await resolver.GetPublicKeysAsync());
            Assert.Equal("tenant-10-app-20-existing", signing.Kid);
            Assert.Equal(signing.Kid, published.Kid);
        }
    }
}
