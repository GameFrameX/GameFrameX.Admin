using GameFrameX.Application.CommerceCenter.GameCommodity;
using GameFrameX.Application.CommerceCenter.GameCommodity.Dto;
using GameFrameX.Application.CommerceCenter.GameGoods;
using GameFrameX.Application.CommerceCenter.GameGoods.Dto;
using GameFrameX.Application.CommerceCenter.GameGoodsCatalogue;
using GameFrameX.Application.CommerceCenter.GameGoodsCatalogue.Dto;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game;
using SqlSugar;

namespace GameFrameX.Core.Tests.GameBusiness;

public sealed class MainDatabaseGoodsServiceTests
{
    [Fact]
    public async Task Goods_CreatePageSelectAndDuplicateGuard_WorkWithoutApplicationContext()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitBusinessTables(typeof(GameGoodsEntity), typeof(GameGoodsCatalogueEntity));
            var repository = new SqlSugarRepository<GameGoodsEntity> { Context = database.MainDb };
            var service = new GameGoodsService(repository);
            var created = await service.AddAsync(new AddGameGoodsEntityRequest
            {
                ConfigId = 1501, ClassifyId = 1, Name = "reward", Description = "reward description",
            });
            Assert.Equal(created.Id, Assert.Single((await service.PageAsync(new PageGameGoodsEntityRequest
            {
                ConfigId = 1501,
            })).Items).Id);
            Assert.Equal(created.Id.ToString(), Assert.Single(await service.SelectStringAsync(new()
            {
                SearchKey = "reward",
            })).Id);
            await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(new AddGameGoodsEntityRequest
            {
                ConfigId = 1501, ClassifyId = 1, Name = "duplicate", Description = "duplicate",
            }));
        }
    }

    [Fact]
    public async Task Catalogue_CreateAndNameUniqueness_WorkWithoutApplicationContext()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitBusinessTables(typeof(GameGoodsCatalogueEntity));
            var service = new GameGoodsCatalogueService(new SqlSugarRepository<GameGoodsCatalogueEntity>
            {
                Context = database.MainDb,
            });
            var created = await service.AddAsync(new AddGameGoodsCatalogueEntityRequest
            {
                Name = "items", Description = "items description",
            });
            Assert.Equal(created.Id, Assert.Single((await service.PageAsync(new PageGameGoodsCatalogueEntityRequest())).Items).Id);
            await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(new AddGameGoodsCatalogueEntityRequest
            {
                Name = "items", Description = "duplicate",
            }));
        }
    }

    [Fact]
    public async Task Commodity_CreateAndFilter_WorkWithoutApplicationContext()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitBusinessTables(typeof(GameCommodityEntity));
            var service = new GameCommodityService(new SqlSugarRepository<GameCommodityEntity>
            {
                Context = database.MainDb,
            });
            var created = await service.AddAsync(new AddGameCommodityEntityRequest
            {
                Status = "available", CategoryId = 1, Number = 5,
            });
            Assert.Equal(created.Id, Assert.Single((await service.PageAsync(new PageGameCommodityEntityRequest
            {
                CategoryId = 1, Status = "available",
            })).Items).Id);
            Assert.Empty((await service.PageAsync(new PageGameCommodityEntityRequest { CategoryId = 2 })).Items);
        }
    }
}
