using GameFrameX.Application.CommerceCenter.GameCommodity;
using GameFrameX.Application.CommerceCenter.GameCommodity.Dto;
using GameFrameX.Application.CommerceCenter.GameGoods;
using GameFrameX.Application.CommerceCenter.GameGoods.Dto;
using GameFrameX.Application.CommerceCenter.GameGoodsCatalogue;
using GameFrameX.Application.CommerceCenter.GameGoodsCatalogue.Dto;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using SqlSugar;

namespace GameFrameX.Core.Tests.GameBusiness;

public sealed class MainDatabaseGoodsServiceTests
{
    [Fact]
    public async Task Goods_CreatePageSelectAndDuplicateGuard_WorkWithoutApplicationContext()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitTenantTables(typeof(TenantGameGoodsEntity), typeof(TenantGameGoodsCatalogueEntity));
            var repository = new SqlSugarRepository<TenantGameGoodsEntity> { Context = database.MainDb };
            var service = new TenantGameGoodsEntityService(repository);
            var created = await service.AddAsync(new AddTenantGameGoodsEntityRequest
            {
                ConfigId = 1501, ClassifyId = 1, Name = "reward", Description = "reward description",
            });
            Assert.Equal(created.Id, Assert.Single((await service.PageAsync(new PageTenantGameGoodsEntityRequest
            {
                ConfigId = 1501,
            })).Items).Id);
            Assert.Equal(created.Id.ToString(), Assert.Single(await service.SelectStringAsync(new()
            {
                SearchKey = "reward",
            })).Id);
            await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(new AddTenantGameGoodsEntityRequest
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
            database.InitTenantTables(typeof(TenantGameGoodsCatalogueEntity));
            var service = new TenantGameGoodsCatalogueEntityService(new SqlSugarRepository<TenantGameGoodsCatalogueEntity>
            {
                Context = database.MainDb,
            });
            var created = await service.AddAsync(new AddTenantGameGoodsCatalogueEntityRequest
            {
                Name = "items", Description = "items description",
            });
            Assert.Equal(created.Id, Assert.Single((await service.PageAsync(new PageTenantGameGoodsCatalogueEntityRequest())).Items).Id);
            await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(new AddTenantGameGoodsCatalogueEntityRequest
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
            database.InitTenantTables(typeof(TenantGameCommodityEntity));
            var service = new TenantGameCommodityEntityService(new SqlSugarRepository<TenantGameCommodityEntity>
            {
                Context = database.MainDb,
            });
            var created = await service.AddAsync(new AddTenantGameCommodityEntityRequest
            {
                Status = "available", CategoryId = 1, Number = 5,
            });
            Assert.Equal(created.Id, Assert.Single((await service.PageAsync(new PageTenantGameCommodityEntityRequest
            {
                CategoryId = 1, Status = "available",
            })).Items).Id);
            Assert.Empty((await service.PageAsync(new PageTenantGameCommodityEntityRequest { CategoryId = 2 })).Items);
        }
    }
}
