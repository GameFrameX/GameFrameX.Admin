using System.Text.Json;
using GameFrameX.Application.Platform;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game;

namespace GameFrameX.Application.SeedData.Game;

/// <summary>
/// 网页商城 表种子数据（default 道具默认下架，道具列表引用 default 游戏物品行）
/// </summary>
public class GameWebShopSeedData : ISqlSugarEntitySeedData<GameWebShopEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GameWebShopEntity> HasData()
    {
        // 道具列表同为 List<GamePropItem> 内嵌 JSON，统一走 EmbeddedJsonOptions 契约序列化
        var itemJson = JsonSerializer.Serialize(new List<GamePropItem>
        {
            new()
            {
                Id = GameBusinessSeedIds.GoodsId,
                ItemId = GameBusinessSeedIds.GoodsConfigId,
                Count = 1,
                Star = 0,
            },
        }, EmbeddedJsonOptions.Instance);

        var records = new List<GameWebShopEntity>
        {
            new()
            {
                Name = "default",
                Description = "默认网页商城道具",
                Logo = string.Empty,
                KeyWords = "gift",
                Price = "0",
                RealPrice = "0",
                ItemJson = itemJson,
                IsUp = false,
                Sales = 0,
                Weight = 10,
                IsEnabled = true,
                CreatedTime = 1745836800,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = GameBusinessSeedIds.WebShopId,
            },
        };

        return records;
    }
}
