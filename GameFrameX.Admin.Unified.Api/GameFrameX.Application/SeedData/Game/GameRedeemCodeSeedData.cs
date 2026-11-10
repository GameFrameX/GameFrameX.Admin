using System.Text.Json;
using GameFrameX.Application.Platform;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game;

namespace GameFrameX.Application.SeedData.Game;

/// <summary>
/// 游戏兑换码 表种子数据（通用演示码：AreaId=0 全区服、ChannelId=null 不限渠道、内容为 default 游戏物品 ×1）
/// </summary>
public class GameRedeemCodeSeedData : ISqlSugarEntitySeedData<GameRedeemCodeEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GameRedeemCodeEntity> HasData()
    {
        var contentsJson = JsonSerializer.Serialize(new List<GamePropItem>
        {
            new()
            {
                Id = GameBusinessSeedIds.GoodsId,
                ItemId = GameBusinessSeedIds.GoodsConfigId,
                Count = 1,
                Star = 0,
            },
        }, EmbeddedJsonOptions.Instance);

        var records = new List<GameRedeemCodeEntity>
        {
            new()
            {
                AreaId = 0,
                ChannelId = null,
                Code = "GFX-DEFAULT-0001",
                Description = "默认通用兑换码",
                IsActive = true,
                UsageLimit = 100,
                UsedCount = 0,
                StartTime = 1704067200,
                EndTime = 4102444800,
                Contents = contentsJson,
                IsEnabled = true,
                CreatedTime = 1745836800,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = GameBusinessSeedIds.RedeemCodeId,
            },
        };

        return records;
    }
}
