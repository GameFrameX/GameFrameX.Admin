using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Device;

namespace GameFrameX.Application.SeedData.Client;

/// <summary>主库默认设备分组。</summary>
public sealed class GameAppDeviceGroupSeedData : ISqlSugarEntitySeedData<GameAppDeviceGroupEntity>
{
    public IEnumerable<GameAppDeviceGroupEntity> HasData()
    {
        return new[]
        {
            new GameAppDeviceGroupEntity
            {
                Id = 31950207082437, Name = "default", Description = "默认缺省分组", IsEnabled = true,
                CreatedTime = 1745836800, CreatedId = 1300000000101, CreatedName = "超级管理员",
            },
        };
    }
}
