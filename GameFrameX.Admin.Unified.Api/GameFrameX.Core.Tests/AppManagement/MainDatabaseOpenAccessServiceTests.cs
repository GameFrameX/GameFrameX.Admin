using System.Globalization;
using GameFrameX.Application.Game.Tenant.OpenAccess.TenantGameAppOpenAccess;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Tenant;
using GameFrameX.Entity.Tenant.Game.App;

namespace GameFrameX.Core.Tests.AppManagement;

public sealed class MainDatabaseOpenAccessServiceTests : IDisposable
{
    private readonly CultureInfo _originalCulture = CultureInfo.CurrentUICulture;

    public MainDatabaseOpenAccessServiceTests()
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("zh-CN");
    }

    public void Dispose()
    {
        CultureInfo.CurrentUICulture = _originalCulture;
    }

    [Fact]
    public async Task CredentialLifecycle_RequiresRealUserAndKeepsRevocationFinal()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitTenantTables(typeof(TenantGameAppOpenAccessEntity), typeof(TenantUserEntity));
            var user = new TenantUserEntity { Id = 100 };
            foreach (var property in typeof(TenantUserEntity).GetProperties().Where(x => x.PropertyType == typeof(string) && x.CanWrite))
            {
                property.SetValue(user, "test");
            }

            database.MainDb.Insertable(user).ExecuteCommand();
            var repository = new SqlSugarRepository<TenantGameAppOpenAccessEntity> { Context = database.MainDb };
            var service = new TenantGameAppOpenAccessService(repository,
                new SqlSugarRepository<TenantUserEntity> { Context = database.MainDb });
            await Assert.ThrowsAnyAsync<Exception>(() => service.AddAsync(new()
            {
                BindUserId = 404, Scenario = "build", Purpose = "test", SubjectName = "builder", ScopesJson = "[\"package-report\"]",
            }));
            var created = await service.AddAsync(new()
            {
                BindUserId = user.Id, Scenario = "build", Purpose = "test", SubjectName = "builder", ScopesJson = "[\"package-report\"]",
            });
            Assert.False(string.IsNullOrWhiteSpace(created.AccessSecret));
            Assert.Single((await service.PageAsync(new() { Scenario = "build" })).Items);
            var stored = await repository.GetFirstAsync(x => x.Id == created.Id);
            await service.RevokeAsync(new() { Id = created.Id, RowVersion = stored.RowVersion, Reason = "retired" });
            stored = await repository.AsQueryable().ClearFilter().FirstAsync(x => x.Id == created.Id);
            Assert.False(stored.IsEnabled);
            Assert.NotNull(stored.RevokedTime);
            var error = await Assert.ThrowsAnyAsync<Exception>(() => service.EnableAsync(new() { Id = stored.Id, RowVersion = stored.RowVersion }));
            Assert.Contains("凭证已撤销，不可启用", error.Message);
        }
    }
}
