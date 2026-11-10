using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game;

namespace GameFrameX.Application.SeedData.Game;

/// <summary>
/// 游戏资源（游戏物品/道具） 表种子数据
/// </summary>
public class GameGoodsSeedData : ISqlSugarEntitySeedData<GameGoodsEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GameGoodsEntity> HasData()
    {
        var records = new List<GameGoodsEntity>
        {
            new()
            {
                Name = "default",
                Description = "默认游戏物品",
                ClassifyId = GameBusinessSeedIds.GoodsCatalogueId,
                ConfigId = GameBusinessSeedIds.GoodsConfigId,
                Star = false,
                IsEnabled = true,
                CreatedTime = 1745836800,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = GameBusinessSeedIds.GoodsId,
            },
        };

        return records;
    }
}
