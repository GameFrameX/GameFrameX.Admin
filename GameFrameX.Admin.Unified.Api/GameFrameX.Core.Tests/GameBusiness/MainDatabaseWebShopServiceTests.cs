using GameFrameX.Application.CommerceCenter.GameWebShop;
using GameFrameX.Application.CommerceCenter.GameWebShop.Dto;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Business.Game;

namespace GameFrameX.Core.Tests.GameBusiness;

public sealed class MainDatabaseWebShopServiceTests
{
    [Fact]
    public async Task CreateAndFilter_PreserveCommodityWithoutApplication()
    {
        using (var database = new ClientApiTestDb())
        using (var client = new HttpClient())
        {
            database.InitBusinessTables(typeof(GameWebShopEntity));
            var service = new GameWebShopService(new SqlSugarRepository<GameWebShopEntity>
            {
                Context = database.MainDb,
            }, client);
            var created = await service.AddAsync(new AddGameWebShopEntityRequest
            {
                Name = "gift", Description = "gift", Logo = "gift.png", KeyWords = "gift",
                Price = "12", RealPrice = "10", ItemJson = "[]", IsUp = true, Weight = 3,
            });
            var row = Assert.Single((await service.PageAsync(new() { Title = "gift" })).Items);
            Assert.Equal(created.Id, row.Id);
            Assert.Equal("10", row.RealPrice);
            Assert.Empty((await service.PageAsync(new() { Title = "missing" })).Items);
        }
    }
}
