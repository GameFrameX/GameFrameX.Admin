using GameFrameX.Application.CommerceCenter.GameOrder;
using GameFrameX.Application.CommerceCenter.GameOrderFulfillment;
using GameFrameX.Core.Service.User;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Business.Game;
using Microsoft.AspNetCore.Http;

namespace GameFrameX.Core.Tests.GameOrder;

public sealed class MainDatabaseOrderServiceTests
{
    [Fact]
    public async Task OrderPageAndSummary_UseBusinessFiltersWithoutApplication()
    {
        using (var database = new ClientApiTestDb())
        {
            var order = database.SeedOrder(101, 201, 10, status: OrderStatusEnum.Shipped, amount: 100, providerTransactionId: "tx-101");
            database.MainDb.Updateable<GameOrderEntity>().SetColumns(x => x.AppleTransactionId == "apple-101").Where(x => x.Id == 101).ExecuteCommand();
            database.SeedOrder(102, 202, 11, status: OrderStatusEnum.Unpaid, amount: 50, providerTransactionId: "tx-102");
            var service = new GameOrderService(new SqlSugarRepository<GameOrderEntity>
            {
                Context = database.MainDb,
            }, null!, new CurrentUserContext(new HttpContextAccessor()));
            Assert.Equal(order.Id, Assert.Single((await service.PageAsync(new() { AreaId = 10 })).Items).Id);
            var summary = await service.SummaryAsync(new() { AreaId = 10 });
            Assert.Equal(1, summary.TotalCount);
            Assert.Equal(100, summary.TotalAmount);
            Assert.Equal(1, summary.PaidCount);
            await Assert.ThrowsAnyAsync<Exception>(() => service.AddAsync(new()));
        }
    }

    [Fact]
    public async Task FulfillmentExport_FiltersMainDatabaseAndRejectsDirectCreation()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitBusinessTables(typeof(GameOrderFulfillmentEntity));
            database.MainDb.Insertable(new GameOrderFulfillmentEntity
            {
                Id = 301, OrderId = 101, AreaId = 10, RoleId = "201", IdempotencyKey = "purchase:101",
                PayChannel = "google", ProviderTransactionId = "tx-101", AccountId = "100", ItemsJson = "[]",
                Type = FulfillmentTypeEnum.Purchase, Status = FulfillmentStatusEnum.Success,
            }).ExecuteCommand();
            var service = new GameOrderFulfillmentService(
                new SqlSugarRepository<GameOrderFulfillmentEntity> { Context = database.MainDb },
                new SqlSugarRepository<GameOrderEntity> { Context = database.MainDb },
                null!, null!, new CurrentUserContext(new HttpContextAccessor()));
            Assert.Single(await service.ExportAsync(new() { AreaId = 10 }));
            Assert.Empty(await service.ExportAsync(new() { AreaId = 11 }));
            await Assert.ThrowsAnyAsync<Exception>(() => service.AddAsync(new()));
        }
    }
}
