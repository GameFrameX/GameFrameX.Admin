using GameFrameX.Application.ClientDelivery.AdvertisementReward;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game;

namespace GameFrameX.Core.Tests.ClientApi;

public sealed class MainDatabaseAdvertisementLedgerTests
{
    [Fact]
    public async Task LedgerPageAndExport_KeepBusinessFiltersAndRejectDirectWrites()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitBusinessTables(typeof(GameAdvertisementRewardEntity));
            database.MainDb.Insertable(new GameAdvertisementRewardEntity
            {
                Id = 100, Provider = "TopOn", TransactionId = "transaction-100", RoleId = "role-100",
                IdempotencyKey = "adreward:topon:transaction-100", AreaId = 10,
            }).ExecuteCommand();
            var service = new GameAdvertisementRewardService(
                new SqlSugarRepository<GameAdvertisementRewardEntity> { Context = database.MainDb });
            Assert.Equal(100, Assert.Single((await service.PageAsync(new() { Provider = "TopOn", AreaId = 10 })).Items).Id);
            Assert.Empty(await service.ExportAsync(new() { Provider = "UnityAds" }));
            Assert.Single(await service.ExportAsync(new() { TransactionId = "transaction-100" }));
            await Assert.ThrowsAnyAsync<Exception>(() => service.AddAsync(new()));
            await Assert.ThrowsAnyAsync<Exception>(() => service.UpdateAsync(new() { Id = 100 }));
        }
    }
}
