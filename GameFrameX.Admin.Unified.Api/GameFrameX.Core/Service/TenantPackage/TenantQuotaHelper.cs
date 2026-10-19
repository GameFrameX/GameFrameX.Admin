using GameFrameX.Core.Service.TenantPackage.Dto;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.System;

namespace GameFrameX.Core.Service.TenantPackage;

/// <summary>
/// 租户限额检查辅助类
/// </summary>
public class TenantQuotaHelper
{
    public static readonly EffectiveQuotaResponse SystemDefaultQuota = new()
    {
        MaxUserCount = 10,
        MaxOrganizationCount = 10,
        MaxOrganizationLevel = 3,
        MaxRoleCount = 5,
        MaxGameAppCount = 1,
        MaxPackageCountPerApp = 1,
        GracePeriodDays = 7,
    };

    private readonly SqlSugarRepository<SystemTenantEntity> _systemTenantRepository;
    private readonly SqlSugarRepository<GlobalPackageEntity> _globalPackageRepository;

    public TenantQuotaHelper(
        SqlSugarRepository<SystemTenantEntity> systemTenantRepository,
        SqlSugarRepository<GlobalPackageEntity> globalPackageRepository)
    {
        _systemTenantRepository = systemTenantRepository;
        _globalPackageRepository = globalPackageRepository;
    }

    /// <summary>
    /// 获取租户的套餐配置，未分配套餐时返回 null
    /// </summary>
    public async Task<GlobalPackageEntity?> GetPackageAsync(long tenantId)
    {
        var tenant = await _systemTenantRepository.GetFirstAsync(u => u.Id == tenantId);
        if (tenant?.PackageId == null)
        {
            return null;
        }

        return await _globalPackageRepository.GetFirstAsync(u => u.Id == tenant.PackageId);
    }

    /// <summary>
    /// 获取租户最终生效配额，未分配套餐时返回系统默认额度
    /// </summary>
    public async Task<EffectiveQuotaResponse> GetEffectiveQuotaAsync(long tenantId)
    {
        var tenant = await _systemTenantRepository.GetFirstAsync(u => u.Id == tenantId);
        if (tenant?.PackageId == null)
        {
            return CreateEffectiveQuota(null, tenant?.GracePeriodDays);
        }

        var package = await _globalPackageRepository.GetFirstAsync(u => u.Id == tenant.PackageId);
        return CreateEffectiveQuota(package, tenant.GracePeriodDays);
    }

    /// <summary>
    /// 组装最终生效配额
    /// </summary>
    public static EffectiveQuotaResponse CreateEffectiveQuota(GlobalPackageEntity? package, int? gracePeriodDaysOverride = null)
    {
        var source = package == null
                         ? CloneSystemDefaultQuota()
                         : new EffectiveQuotaResponse
                         {
                             MaxUserCount = package.MaxUserCount,
                             MaxOrganizationCount = package.MaxOrganizationCount,
                             MaxOrganizationLevel = package.MaxOrganizationLevel,
                             MaxRoleCount = package.MaxRoleCount,
                             MaxGameAppCount = package.MaxGameAppCount,
                             MaxPackageCountPerApp = package.MaxPackageCountPerApp,
                             GracePeriodDays = package.GracePeriodDays,
                         };

        source.GracePeriodDays = gracePeriodDaysOverride ?? source.GracePeriodDays;
        return source;
    }

    private static EffectiveQuotaResponse CloneSystemDefaultQuota()
    {
        return new EffectiveQuotaResponse
        {
            MaxUserCount = SystemDefaultQuota.MaxUserCount,
            MaxOrganizationCount = SystemDefaultQuota.MaxOrganizationCount,
            MaxOrganizationLevel = SystemDefaultQuota.MaxOrganizationLevel,
            MaxRoleCount = SystemDefaultQuota.MaxRoleCount,
            MaxGameAppCount = SystemDefaultQuota.MaxGameAppCount,
            MaxPackageCountPerApp = SystemDefaultQuota.MaxPackageCountPerApp,
            GracePeriodDays = SystemDefaultQuota.GracePeriodDays,
        };
    }
}