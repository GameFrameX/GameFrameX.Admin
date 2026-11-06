using GameFrameX.Core.Middleware;
using GameFrameX.Entity.Business.Game;

namespace GameFrameX.Core.Tests.ClientApi;

public sealed class KeyRingSingleMainTests
{
    [Fact]
    public async Task SignatureRotation_SeesNewKeyAndDisablement_WithoutRequestScope()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitBusinessTables(typeof(AppSignatureKeyEntity));
            database.MainDb.Insertable(new AppSignatureKeyEntity
            {
                Id = 1, Kid = "old", Secret = "old-secret", IsEnabled = true, CreatedTime = 1, UpdateTime = 0,
            }).ExecuteCommand();
            var resolver = new SignatureKeyResolver(database.MainDb);
            Assert.Equal("old-secret", (await resolver.ResolveAsync("old")).Secret);
            database.MainDb.Insertable(new AppSignatureKeyEntity
            {
                Id = 2, Kid = "new", Secret = "new-secret", IsEnabled = true, CreatedTime = 2, UpdateTime = 0,
            }).ExecuteCommand();
            Assert.Equal(new[] { "new", "old" }, (await resolver.GetActiveKeysAsync()).Select(key => key.Kid));
            database.MainDb.Updateable<AppSignatureKeyEntity>()
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
            database.InitBusinessTables(typeof(SessionKeyEntity));
            database.MainDb.Insertable(new SessionKeyEntity
            {
                Id = 1, Kid = "old", Secret = "old-secret", IsEnabled = true, CreatedTime = 1, UpdateTime = 0,
            }).ExecuteCommand();
            var resolver = new SessionKeyResolver(database.MainDb);
            Assert.Equal("old-secret", (await resolver.ResolveAsync("old")).Secret);
            database.MainDb.Insertable(new SessionKeyEntity
            {
                Id = 2, Kid = "new", Secret = "new-secret", IsEnabled = true, CreatedTime = 2, UpdateTime = 0,
            }).ExecuteCommand();
            Assert.Equal("new-secret", (await resolver.ResolveAsync("new")).Secret);
            database.MainDb.Updateable<SessionKeyEntity>()
                .SetColumns(key => key.ExpireTime == 1).SetColumns(key => key.UpdateTime == 3)
                .Where(key => key.Id == 1).ExecuteCommand();
            Assert.Equal(SessionKeyLookupResult.Expired, (await resolver.ResolveAsync("old")).Result);
            Assert.Null((await resolver.ResolveAsync("old")).Secret);
            Assert.Equal(SessionKeyLookupResult.NotFound, (await resolver.ResolveAsync("missing")).Result);
        }
    }
}
