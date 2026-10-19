using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Global;

namespace GameFrameX.Application.SeedData.System;

/// <summary>
/// 系统租户套餐等级表种子数据
/// </summary>
/// <remarks>
/// 套餐为系统级产品定义，作为版本基线由代码管控；
/// 重启时 SqlSugar 会按 Id upsert 覆盖，运营层不应通过 UI 修改。
/// 新增套餐档位：追加条目并分配新的固定 Id；发布后 Code 不可改名。
/// </remarks>
public class GlobalPackageSeedData : ISqlSugarEntitySeedData<GlobalPackageEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GlobalPackageEntity> HasData()
    {
        var createdTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds();

        return new[]
        {
            new GlobalPackageEntity
            {
                Id = StableSeedId.TenantPackage("default"),
                Name = "默认套餐",
                Code = "default",
                MaxUserCount = 10,
                MaxOrganizationCount = 10,
                MaxOrganizationLevel = 3,
                MaxRoleCount = 5,
                MaxGameAppCount = 1,
                MaxPackageCountPerApp = 1,
                GracePeriodDays = 7,
                OrderNo = 1,
                Remark = "兑底套餐：未指定套餐时使用",
                IsEnabled = true,
                CreatedTime = createdTime,
                UpdateCount = 0,
            },
            new GlobalPackageEntity
            {
                Id = StableSeedId.TenantPackage("free"),
                Name = "免费版",
                Code = "free",
                MaxUserCount = 5,
                MaxOrganizationCount = 3,
                MaxOrganizationLevel = 2,
                MaxRoleCount = 3,
                MaxGameAppCount = 1,
                MaxPackageCountPerApp = 2,
                GracePeriodDays = 0,
                OrderNo = 10,
                Remark = "免费档：最小可用配额",
                IsEnabled = true,
                CreatedTime = createdTime,
                UpdateCount = 0,
            },
            new GlobalPackageEntity
            {
                Id = StableSeedId.TenantPackage("pro"),
                Name = "专业版",
                Code = "pro",
                MaxUserCount = 20,
                MaxOrganizationCount = 10,
                MaxOrganizationLevel = 3,
                MaxRoleCount = 10,
                MaxGameAppCount = 5,
                MaxPackageCountPerApp = 20,
                GracePeriodDays = 7,
                OrderNo = 20,
                Remark = "专业档：中小团队常用",
                IsEnabled = true,
                CreatedTime = createdTime,
                UpdateCount = 0,
            },
            new GlobalPackageEntity
            {
                Id = StableSeedId.TenantPackage("enterprise"),
                Name = "企业版",
                Code = "enterprise",
                MaxUserCount = 100,
                MaxOrganizationCount = 50,
                MaxOrganizationLevel = 5,
                MaxRoleCount = 30,
                MaxGameAppCount = 30,
                MaxPackageCountPerApp = 50,
                GracePeriodDays = 15,
                OrderNo = 30,
                Remark = "企业档：大规模组织",
                IsEnabled = true,
                CreatedTime = createdTime,
                UpdateCount = 0,
            },
        };
    }
}