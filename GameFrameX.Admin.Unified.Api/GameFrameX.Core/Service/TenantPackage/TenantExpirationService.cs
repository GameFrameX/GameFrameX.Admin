using GameFrameX.Entity.Global;
using GameFrameX.Entity.System;

namespace GameFrameX.Core.Service.TenantPackage;

/// <summary>
/// 租户到期状态计算服务
/// </summary>
public class TenantExpirationService
{
    private readonly SqlSugarRepository<SystemTenantEntity> _systemTenantRepository;
    private readonly SqlSugarRepository<GlobalPackageEntity> _globalPackageRepository;

    public TenantExpirationService(
        SqlSugarRepository<SystemTenantEntity> systemTenantRepository,
        SqlSugarRepository<GlobalPackageEntity> globalPackageRepository)
    {
        _systemTenantRepository = systemTenantRepository;
        _globalPackageRepository = globalPackageRepository;
    }

    /// <summary>
    /// 获取租户到期状态（按租户 ID 查库计算）。
    /// </summary>
    public async Task<TenantExpirationStatus> GetExpirationStatusAsync(long tenantId)
    {
        var tenant = await _systemTenantRepository.GetFirstAsync(u => u.Id == tenantId);
        if (tenant == null)
        {
            return TenantExpirationStatus.Active;
        }

        GlobalPackageEntity? package = null;
        if (tenant.PackageId.HasValue)
        {
            package = await _globalPackageRepository.GetFirstAsync(u => u.Id == tenant.PackageId.Value);
        }

        return GetExpirationStatus(tenant, package);
    }

    /// <summary>
    /// 基于已加载的租户与套餐实体计算到期状态（纯计算，不查库）。
    /// <para>供 <c>TenantAccessGate</c> 等运行期门禁复用，避免每请求重复查库；套餐仅在租户未显式设置过渡期时用于取默认过渡期天数。</para>
    /// </summary>
    /// <param name="tenant">已加载的租户实体；为 null 视为正常。</param>
    /// <param name="package">租户套餐实体；可为 null（按 0 天过渡期处理）。</param>
    /// <returns>到期状态。</returns>
    public TenantExpirationStatus GetExpirationStatus(SystemTenantEntity? tenant, GlobalPackageEntity? package)
    {
        if (tenant == null || tenant.ExpireTime == null)
        {
            return TenantExpirationStatus.Active;
        }

        var expireTime = tenant.ExpireTime.Value;
        var now = TimerHelper.UnixTimeSeconds();

        // 未到期
        if (expireTime > now)
        {
            return TenantExpirationStatus.Active;
        }

        // 已到期，计算过渡期截止日
        int gracePeriodDays;
        if (tenant.GracePeriodDays.HasValue)
        {
            gracePeriodDays = tenant.GracePeriodDays.Value;
        }
        else
        {
            gracePeriodDays = package?.GracePeriodDays ?? 0;
        }

        var gracePeriodEnd = expireTime + gracePeriodDays * 24 * 60 * 60;

        // 过渡期内
        if (now <= gracePeriodEnd)
        {
            return TenantExpirationStatus.InGracePeriod;
        }

        // 过渡期结束
        return TenantExpirationStatus.Expired;
    }
}