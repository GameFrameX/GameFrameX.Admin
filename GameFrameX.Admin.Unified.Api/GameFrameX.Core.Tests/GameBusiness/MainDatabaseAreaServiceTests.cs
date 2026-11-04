using GameFrameX.Application.ServerCenter.GameArea;
using GameFrameX.Application.ServerCenter.GameAreaClassify;
using GameFrameX.Application.ServerCenter.GameAreaClassify.Dto;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Core.Tests.GameBusiness;

public sealed class MainDatabaseAreaServiceTests
{
    [Fact]
    public async Task DisabledArea_RemainsVisibleForManagementWithoutApplication()
    {
        using (var database = new ClientApiTestDb())
        using (var client = new HttpClient())
        {
            database.SeedAreaClassify(100);
            var area = database.SeedArea(101, 100, isEnabled: false);
            database.MainDb.QueryFilter.AddTableFilter<TenantGameAreaEntity>(x => x.IsEnabled == true);
            var service = new TenantGameAreaEntityService(new SqlSugarRepository<TenantGameAreaEntity>
            {
                Context = database.MainDb,
            }, new SqlSugarRepository<TenantGameAreaClassifyEntity> { Context = database.MainDb }, client);
            var row = Assert.Single((await service.PageAsync(new() { IsEnabled = false })).Items);
            Assert.Equal(area.Id, row.Id);
            Assert.Equal("Asia", row.ClassifyIdName);
            Assert.False(row.IsEnabled);
            Assert.Equal(area.Id, (await service.GetAsync(new() { Id = area.Id })).Id);
        }
    }

    [Fact]
    public async Task Classification_UpdateWithEmptyPassword_PreservesStoredSecret()
    {
        using (var database = new ClientApiTestDb())
        {
            var existing = database.SeedAreaClassify(100);
            var service = new TenantGameAreaClassifyEntityService(new SqlSugarRepository<TenantGameAreaClassifyEntity>
            {
                Context = database.MainDb,
            });
            await service.UpdateAsync(new UpdateTenantGameAreaClassifyEntityRequest
            {
                Id = existing.Id, RowVersion = existing.RowVersion, Name = "updated", Description = "updated",
                DockerPassword = "",
            });
            var updated = await database.MainDb.Queryable<TenantGameAreaClassifyEntity>().InSingleAsync(existing.Id);
            Assert.Equal("updated", updated.Name);
            Assert.Equal(existing.DockerPassword, updated.DockerPassword);
        }
    }
}
