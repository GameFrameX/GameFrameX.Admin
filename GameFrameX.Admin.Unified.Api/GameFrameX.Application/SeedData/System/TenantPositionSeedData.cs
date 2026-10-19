using GameFrameX.Core.Const;
using GameFrameX.Core.SqlSugar;

namespace GameFrameX.Application.SeedData.System;

/// <summary>
/// 系统职位表种子数据
/// </summary>
public class TenantPositionSeedData : ISqlSugarEntitySeedData<TenantPositionEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TenantPositionEntity> HasData()
    {
        var createdTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds();

        return TenantDefaultDataTemplates.PositionTemplates.Select(template => new TenantPositionEntity
        {
            Id = template.SeedId,
            Name = template.Name,
            Code = template.Code,
            OrderNo = template.OrderNo,
            CreatedTime = createdTime,
            Remark = template.Remark,
            TenantId = TenantDefaultDataTemplates.MainTenantId,
            UpdateCount = 0,
        }).ToList();
    }
}