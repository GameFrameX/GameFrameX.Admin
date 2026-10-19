using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Global;

namespace GameFrameX.Application.SeedData.System;

/// <summary>
/// 系统字典类型表种子数据
/// </summary>
public class GlobalDictionaryTypeSeedData : ISqlSugarEntitySeedData<GlobalDictionaryTypeEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GlobalDictionaryTypeEntity> HasData()
    {
        return new[]
        {
            new GlobalDictionaryTypeEntity { Id = 1300000000106, Name = "运营组织类型", Code = "org_type", OrderNo = 105, Remark = "游戏行业运营组织类型", IsEnabled = true, CreatedTime = DateTimeOffset.Parse("2023-02-10 00:00:00").ToUnixTimeSeconds(), },
        };
    }
}