using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game.FeatureSwitch;

namespace GameFrameX.Application.SeedData.Game;

/// <summary>
/// 活动/功能开关注册表 表种子数据（maintenance 默认行：全作用域、参数为空对象，由消费方按需解析）
/// </summary>
public class GameAppFeatureSwitchSeedData : ISqlSugarEntitySeedData<GameAppFeatureSwitchEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GameAppFeatureSwitchEntity> HasData()
    {
        var records = new List<GameAppFeatureSwitchEntity>
        {
            new()
            {
                SwitchKey = "maintenance",
                DisplayName = "维护模式",
                Description = "默认维护模式开关键，参数由消费方解析",
                Audience = "both",
                ValueJson = "{}",
                StartTime = null,
                EndTime = null,
                Platform = "default",
                Package = "default",
                Language = "default",
                Channel = "default",
                Priority = 0,
                RolloutPercent = 100,
                IsEnabled = true,
                CreatedTime = 1745836800,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = GameBusinessSeedIds.FeatureSwitchId,
            },
        };

        return records;
    }
}
