using GameFrameX.Application.Game.TimeJob.GameTimeJob;
using GameFrameX.Application.Game.TimeJob.GameTimeJob.Dto;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Core.Tests.GameBusiness;

public sealed class MainDatabaseTimeJobServiceTests
{
    [Fact]
    public async Task CreateAndFilter_ForceManualSourceWithoutApplication()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitTenantTables(typeof(TenantGameTimeJobEntity));
            var service = new TenantGameTimeJobEntityService(new SqlSugarRepository<TenantGameTimeJobEntity>
            {
                Context = database.MainDb,
            });
            var created = await service.AddAsync(new AddTenantGameTimeJobEntityRequest
            {
                Name = "daily", Description = "daily", Cron = "0 0 1 * * ?", Source = "forged",
            });
            var row = Assert.Single((await service.PageAsync(new() { Name = "daily" })).Items);
            Assert.Equal(created.Id, row.Id);
            Assert.Equal("Manual", row.Source);
            Assert.Empty((await service.PageAsync(new() { Name = "missing" })).Items);
        }
    }
}
