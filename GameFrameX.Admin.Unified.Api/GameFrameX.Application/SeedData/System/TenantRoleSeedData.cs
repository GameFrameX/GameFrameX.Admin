using GameFrameX.Core.Const;
using GameFrameX.Core.SqlSugar;

namespace GameFrameX.Application.SeedData.System;

/// <summary>
/// 系统角色表种子数据
/// </summary>
public class TenantRoleSeedData : ISqlSugarEntitySeedData<TenantRoleEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TenantRoleEntity> HasData()
    {
        var createdTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds();

        return TenantDefaultDataTemplates.RoleTemplates.Select(template => new TenantRoleEntity
        {
            Id = template.SeedId,
            Name = template.Name,
            DataScope = template.DataScope,
            Code = template.Code,
            CreatedTime = createdTime,
            Remark = template.Remark,
            TenantId = TenantDefaultDataTemplates.MainTenantId,
            UpdateCount = 0,
            OrderNo = template.OrderNo,
        }).ToList();
    }
}