using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game;

namespace GameFrameX.Application.SeedData.Game;

/// <summary>
/// 奖励模板分组 表种子数据
/// </summary>
public class GameRewardTemplateGroupSeedData : ISqlSugarEntitySeedData<GameRewardTemplateGroupEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GameRewardTemplateGroupEntity> HasData()
    {
        var records = new List<GameRewardTemplateGroupEntity>
        {
            new()
            {
                Name = "default",
                Description = "默认奖励模板分组",
                Sort = 0,
                IsEnabled = true,
                CreatedTime = 1745836800,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = GameBusinessSeedIds.RewardTemplateGroupId,
            },
        };

        return records;
    }
}
