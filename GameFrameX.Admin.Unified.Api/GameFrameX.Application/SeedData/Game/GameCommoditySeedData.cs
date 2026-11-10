using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game;

namespace GameFrameX.Application.SeedData.Game;

/// <summary>
/// 商品（游戏内商品） 表种子数据
/// </summary>
public class GameCommoditySeedData : ISqlSugarEntitySeedData<GameCommodityEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GameCommodityEntity> HasData()
    {
        var records = new List<GameCommodityEntity>
        {
            new()
            {
                Status = "OnSale",
                CategoryId = GameBusinessSeedIds.GoodsCatalogueId,
                Cost = false,
                Number = 1,
                CreatedTime = 1745836800,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = GameBusinessSeedIds.CommodityId,
            },
        };

        return records;
    }
}
