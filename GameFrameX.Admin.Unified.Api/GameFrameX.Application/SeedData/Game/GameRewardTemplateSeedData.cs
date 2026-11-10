using System.Text.Json;
using GameFrameX.Application.Platform;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game;

namespace GameFrameX.Application.SeedData.Game;

/// <summary>
/// 奖励模板 表种子数据（奖励明细引用 default 游戏物品行，快照结构版本固定 1）
/// </summary>
public class GameRewardTemplateSeedData : ISqlSugarEntitySeedData<GameRewardTemplateEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GameRewardTemplateEntity> HasData()
    {
        // 奖励明细与兑换码内容同为 List<GamePropItem> 内嵌 JSON，统一走 EmbeddedJsonOptions 序列化
        // （长整型 Id 契约字符串化、PascalCase 键，与后端存储和游戏客户端契约一致）
        var rewardItemsJson = JsonSerializer.Serialize(new List<GamePropItem>
        {
            new()
            {
                Id = GameBusinessSeedIds.GoodsId,
                ItemId = GameBusinessSeedIds.GoodsConfigId,
                Count = 1,
                Star = 0,
            },
        }, EmbeddedJsonOptions.Instance);

        var records = new List<GameRewardTemplateEntity>
        {
            new()
            {
                Name = "default",
                Description = "默认奖励模板",
                GroupId = GameBusinessSeedIds.RewardTemplateGroupId,
                RewardItemsJson = rewardItemsJson,
                SnapshotSchemaVersion = 1,
                Sort = 0,
                IsEnabled = true,
                CreatedTime = 1745836800,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = GameBusinessSeedIds.RewardTemplateId,
            },
        };

        return records;
    }
}
