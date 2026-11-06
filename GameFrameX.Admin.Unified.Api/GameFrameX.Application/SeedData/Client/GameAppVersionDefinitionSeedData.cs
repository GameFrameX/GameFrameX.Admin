using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game.Release;

namespace GameFrameX.Application.SeedData.Client;

/// <summary>主库默认版本定义。</summary>
public sealed class GameAppVersionDefinitionSeedData : ISqlSugarEntitySeedData<GameAppVersionDefinitionEntity>
{
    public IEnumerable<GameAppVersionDefinitionEntity> HasData()
    {
        return new[] { "default", "1.0.0" }.Select((name, index) => new GameAppVersionDefinitionEntity
        {
            Id = 31950207182437 + index, Name = name, Description = index == 0 ? "默认缺省版本" : "初始版本",
            IsEnabled = true, CreatedTime = 1745836800, CreatedId = 1300000000101, CreatedName = "超级管理员",
        });
    }
}
