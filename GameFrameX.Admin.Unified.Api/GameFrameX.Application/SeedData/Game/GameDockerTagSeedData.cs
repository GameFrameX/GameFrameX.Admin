using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game;

namespace GameFrameX.Application.SeedData.Game;

/// <summary>
/// 游戏区服镜像标签 表种子数据
/// </summary>
public class GameDockerTagSeedData : ISqlSugarEntitySeedData<GameDockerTagEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GameDockerTagEntity> HasData()
    {
        var records = new List<GameDockerTagEntity>
        {
            new()
            {
                Name = "default",
                Description = "默认镜像标签",
                ServerVersion = "1.0.0",
                IsDefault = true,
                IsEnabled = true,
                CreatedTime = 1745836800,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = GameBusinessSeedIds.DockerTagId,
            },
        };

        return records;
    }
}
