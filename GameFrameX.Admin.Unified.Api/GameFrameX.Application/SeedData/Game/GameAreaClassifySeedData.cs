using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game;

namespace GameFrameX.Application.SeedData.Game;

/// <summary>
/// 区服分类 表种子数据
/// </summary>
public class GameAreaClassifySeedData : ISqlSugarEntitySeedData<GameAreaClassifyEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GameAreaClassifyEntity> HasData()
    {
        var records = new List<GameAreaClassifyEntity>
        {
            new()
            {
                Name = "default",
                Description = "默认缺省大区分类",
                LocalizationKey = "area_default",
                HttpManagerUrl = string.Empty,
                DockerRegister = string.Empty,
                ImageName = string.Empty,
                DockerUserName = string.Empty,
                DockerPassword = string.Empty,
                ContainerNetName = string.Empty,
                ContainerSubnet = string.Empty,
                ContainerGateway = string.Empty,
                IsEnabled = true,
                CreatedTime = 1745836800,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = GameBusinessSeedIds.AreaClassifyId,
            },
        };

        return records;
    }
}
