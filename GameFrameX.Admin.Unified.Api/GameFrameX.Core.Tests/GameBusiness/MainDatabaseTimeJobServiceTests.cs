using GameFrameX.Application.Platform.TimeJob.GameTimeJob;
using GameFrameX.Application.Platform.TimeJob.GameTimeJob.Dto;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Business.Game;

namespace GameFrameX.Core.Tests.GameBusiness;

public sealed class MainDatabaseTimeJobServiceTests
{
    [Fact]
    public async Task CreateAndFilter_ForceManualSourceWithoutApplication()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitBusinessTables(typeof(GameTimeJobEntity));
            var service = new GameTimeJobService(new SqlSugarRepository<GameTimeJobEntity>
            {
                Context = database.MainDb,
            });
            var created = await service.AddAsync(new AddGameTimeJobEntityRequest
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
