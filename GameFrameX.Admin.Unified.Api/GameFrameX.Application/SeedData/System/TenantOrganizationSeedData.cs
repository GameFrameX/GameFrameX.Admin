using GameFrameX.Core.Const;
using GameFrameX.Core.SqlSugar;

namespace GameFrameX.Application.SeedData.System;

/// <summary>
/// 系统机构表种子数据
/// </summary>
public class TenantOrganizationSeedData : ISqlSugarEntitySeedData<TenantOrganizationEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TenantOrganizationEntity> HasData()
    {
        var createdTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds();
        var orgsByKey = new Dictionary<string, TenantOrganizationEntity>(StringComparer.Ordinal);

        return TenantDefaultDataTemplates.OrganizationTemplates.Select(template =>
        {
            var org = new TenantOrganizationEntity
            {
                Id = template.SeedId,
                Pid = template.ParentKey == null ? 0 : orgsByKey[template.ParentKey].Id,
                Name = template.Name,
                Code = template.Code,
                Type = template.Type,
                Level = template.Level,
                OrderNo = template.OrderNo,
                CreatedTime = createdTime,
                Remark = template.Remark,
                TenantId = TenantDefaultDataTemplates.MainTenantId,
                UpdateCount = 0,
            };
            orgsByKey.Add(template.Key, org);
            return org;
        }).ToList();
    }
}