using GameFrameX.Core.Middleware;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Core.Tests.ClientApi;

public sealed class KeyResolverQueryTests
{
    [Fact]
    public async Task Lookup_OnlyLoadsRequestedKey_AndNeverDecryptsInactiveRows()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitTenantTables(typeof(TenantSessionKeyEntity), typeof(TenantAppSignatureKeyEntity));
            database.MainDb.Insertable(new[]
            {
                new TenantSessionKeyEntity { Id = 1, Kid = "active", Secret = "active-secret", IsEnabled = true },
                new TenantSessionKeyEntity { Id = 2, Kid = "unrelated", Secret = "enc:sec:v1:invalid", IsEnabled = false },
            }).ExecuteCommand();
            database.MainDb.Insertable(new[]
            {
                new TenantAppSignatureKeyEntity { Id = 1, Kid = "active", Secret = "active-secret", IsEnabled = true },
                new TenantAppSignatureKeyEntity { Id = 2, Kid = "disabled", Secret = "enc:sec:v1:invalid", IsEnabled = false },
                new TenantAppSignatureKeyEntity { Id = 3, Kid = "expired", Secret = "enc:sec:v1:invalid", IsEnabled = true, ExpireTime = 1 },
            }).ExecuteCommand();
            var statements = new List<string>();
            database.MainDb.Aop.OnLogExecuting = (sql, parameters) => statements.Add(sql);
            var session = new SessionKeyResolver(database.MainDb);
            var signature = new SignatureKeyResolver(database.MainDb);
            Assert.Equal("active-secret", (await session.ResolveAsync("active")).Secret);
            Assert.Equal("active-secret", (await signature.ResolveAsync("active")).Secret);
            Assert.All(statements, sql => Assert.Contains("WHERE", sql, StringComparison.OrdinalIgnoreCase));
            Assert.Equal(SignatureKeyLookupResult.Disabled, (await signature.ResolveAsync("disabled")).Result);
            Assert.Equal("active", Assert.Single(await signature.GetActiveKeysAsync()).Kid);
            database.MainDb.Updateable<TenantAppSignatureKeyEntity>().SetColumns(key => key.Secret == "changed-secret").Where(key => key.Id == 1).ExecuteCommand();
            Assert.Equal("changed-secret", (await signature.ResolveAsync("active")).Secret);
            database.MainDb.Deleteable<TenantSessionKeyEntity>().Where(key => key.Id == 1).ExecuteCommand();
            Assert.Equal(SessionKeyLookupResult.NotFound, (await session.ResolveAsync("active")).Result);
        }
    }
}
