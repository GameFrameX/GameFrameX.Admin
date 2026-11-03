using GameFrameX.Core.Middleware;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Core.Tests.ClientApi;

public sealed class KeyRingSingleMainTests
{
    [Fact]
    public async Task SignatureRotation_SeesNewKeyAndDisablement_WithoutRequestScope()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitTenantTables(typeof(TenantAppSignatureKeyEntity));
            database.MainDb.Insertable(new TenantAppSignatureKeyEntity
            {
                Id = 1, Kid = "old", Secret = "old-secret", IsEnabled = true, CreatedTime = 1, UpdateTime = 0,
            }).ExecuteCommand();
            var resolver = new SignatureKeyResolver(database.MainDb);
            Assert.Equal("old-secret", (await resolver.ResolveAsync("old")).Secret);
            database.MainDb.Insertable(new TenantAppSignatureKeyEntity
            {
                Id = 2, Kid = "new", Secret = "new-secret", IsEnabled = true, CreatedTime = 2, UpdateTime = 0,
            }).ExecuteCommand();
            Assert.Equal(new[] { "new", "old" }, (await resolver.GetActiveKeysAsync()).Select(key => key.Kid));
            database.MainDb.Updateable<TenantAppSignatureKeyEntity>()
                .SetColumns(key => key.IsEnabled == false).SetColumns(key => key.UpdateTime == 3)
                .Where(key => key.Id == 1).ExecuteCommand();
            Assert.Equal(SignatureKeyLookupResult.Disabled, (await resolver.ResolveAsync("old")).Result);
            Assert.Null((await resolver.ResolveAsync("old")).Secret);
            Assert.Equal(SignatureKeyLookupResult.NotFound, (await resolver.ResolveAsync("missing")).Result);
        }
    }

    [Fact]
    public async Task SessionRotation_SeesNewKeyAndLeaseChanges_WithoutRequestScope()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitTenantTables(typeof(TenantSessionKeyEntity));
            database.MainDb.Insertable(new TenantSessionKeyEntity
            {
                Id = 1, Kid = "old", Secret = "old-secret", IsEnabled = true, CreatedTime = 1, UpdateTime = 0,
            }).ExecuteCommand();
            var resolver = new SessionKeyResolver(database.MainDb);
            Assert.Equal("old-secret", (await resolver.ResolveAsync("old")).Secret);
            database.MainDb.Insertable(new TenantSessionKeyEntity
            {
                Id = 2, Kid = "new", Secret = "new-secret", IsEnabled = true, CreatedTime = 2, UpdateTime = 0,
            }).ExecuteCommand();
            Assert.Equal("new-secret", (await resolver.ResolveAsync("new")).Secret);
            database.MainDb.Updateable<TenantSessionKeyEntity>()
                .SetColumns(key => key.ExpireTime == 1).SetColumns(key => key.UpdateTime == 3)
                .Where(key => key.Id == 1).ExecuteCommand();
            Assert.Equal(SessionKeyLookupResult.Expired, (await resolver.ResolveAsync("old")).Result);
            Assert.Null((await resolver.ResolveAsync("old")).Secret);
            Assert.Equal(SessionKeyLookupResult.NotFound, (await resolver.ResolveAsync("missing")).Result);
        }
    }
}
