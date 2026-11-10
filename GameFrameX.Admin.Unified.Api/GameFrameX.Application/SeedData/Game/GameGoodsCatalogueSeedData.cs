using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game;

namespace GameFrameX.Application.SeedData.Game;

/// <summary>
/// 游戏资源分类（物品分组） 表种子数据
/// </summary>
public class GameGoodsCatalogueSeedData : ISqlSugarEntitySeedData<GameGoodsCatalogueEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GameGoodsCatalogueEntity> HasData()
    {
        var records = new List<GameGoodsCatalogueEntity>
        {
            new()
            {
                Name = "default",
                Description = "默认物品分组",
                IsEnabled = true,
                CreatedTime = 1745836800,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = GameBusinessSeedIds.GoodsCatalogueId,
            },
        };

        return records;
    }
}
