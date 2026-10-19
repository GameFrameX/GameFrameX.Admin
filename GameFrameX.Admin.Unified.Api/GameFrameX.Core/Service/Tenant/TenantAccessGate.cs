using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.TenantPackage;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.System;

namespace GameFrameX.Core.Service.Tenant;

/// <summary>
/// 租户访问门禁：按租户启用状态 + 到期状态 + 读写动作，裁决当前请求是否放行。
/// <para>裁决矩阵：</para>
/// <list type="bullet">
/// <item>租户禁用（<c>IsEnabled==false</c>）→ <see cref="TenantAccessVerdict.DenyDisabled"/>（禁读禁写，Z1003）。</item>
/// <item>到期状态 Expired 且写操作 → <see cref="TenantAccessVerdict.DenyExpiredWrite"/>（Z1007）；Expired 且读 → <see cref="TenantAccessVerdict.Allow"/>。</item>
/// <item>Active / InGracePeriod → <see cref="TenantAccessVerdict.Allow"/>（过渡期内仍正常使用，不挡写）。</item>
/// </list>
/// <para>租户读取优先走 <see cref="CacheConst.KeyTenant"/> 缓存，缓存未命中回退查库。</para>
/// <para>调用方负责先放行 SuperAdmin、未认证请求与公开端点；本门禁只裁决「已确认的普通租户请求」。</para>
/// </summary>
public class TenantAccessGate
{
    private readonly SystemCacheService _systemCacheService;
    private readonly SqlSugarRepository<SystemTenantEntity> _systemTenantRepository;
    private readonly SqlSugarRepository<GlobalPackageEntity> _systemPackageRepository;
    private readonly TenantExpirationService _tenantExpirationService;

    public TenantAccessGate(SystemCacheService systemCacheService,
        SqlSugarRepository<SystemTenantEntity> systemTenantRepository,
        SqlSugarRepository<GlobalPackageEntity> systemPackageRepository,
        TenantExpirationService tenantExpirationService)
    {
        _systemCacheService = systemCacheService;
        _systemTenantRepository = systemTenantRepository;
        _systemPackageRepository = systemPackageRepository;
        _tenantExpirationService = tenantExpirationService;
    }

    /// <summary>
    /// 评估当前租户请求的访问裁决。
    /// </summary>
    /// <param name="tenantId">当前请求有效租户 ID（由调用方从登录态或请求上下文解析；SuperAdmin / 未认证应已在上游放行）。</param>
    /// <param name="isWriteAction">是否为写操作（由 <see cref="TenantReadActionClassifier.IsReadAction"/> 取反得出）。</param>
    /// <returns>访问裁决。</returns>
    public async Task<TenantAccessVerdict> EvaluateAsync(long tenantId, bool isWriteAction)
    {
        var tenantAccess = await LoadTenantAsync(tenantId);
        var status = _tenantExpirationService.GetExpirationStatus(tenantAccess.Tenant, tenantAccess.Package);
        return Decide(tenantAccess.Tenant, status, isWriteAction);
    }

    /// <summary>
    /// 纯裁决逻辑（无 IO），供单测直接覆盖裁决矩阵。
    /// <para>裁决矩阵：</para>
    /// <list type="bullet">
    /// <item><c>tenant==null</c> → <see cref="TenantAccessVerdict.Allow"/>。</item>
    /// <item><c>IsEnabled==false</c> → <see cref="TenantAccessVerdict.DenyDisabled"/>（禁读禁写）。</item>
    /// <item>过期且写 → <see cref="TenantAccessVerdict.DenyExpiredWrite"/>。</item>
    /// <item>其余（Active / InGracePeriod / Expired 读 / IsEnabled==null 视为启用）→ <see cref="TenantAccessVerdict.Allow"/>。</item>
    /// </list>
    /// </summary>
    public static TenantAccessVerdict Decide(SystemTenantEntity? tenant, TenantExpirationStatus status, bool isWriteAction)
    {
        if (tenant == null)
        {
            return TenantAccessVerdict.Allow;
        }

        if (tenant.IsEnabled == false)
        {
            return TenantAccessVerdict.DenyDisabled;
        }

        if (status == TenantExpirationStatus.Expired && isWriteAction)
        {
            return TenantAccessVerdict.DenyExpiredWrite;
        }

        return TenantAccessVerdict.Allow;
    }

    private async Task<TenantAccessContext> LoadTenantAsync(long tenantId)
    {
        var cached = _systemCacheService.Get<List<SystemTenantEntity>>(CacheConst.KeyTenant);
        if (cached != null)
        {
            var tenant = cached.FirstOrDefault(u => u.Id == tenantId);
            if (tenant != null)
            {
                return new TenantAccessContext
                {
                    Tenant = tenant,
                    Package = await LoadPackageIfNeededAsync(tenant),
                };
            }
        }

        var dbTenant = await _systemTenantRepository.GetFirstAsync(u => u.Id == tenantId);
        if (dbTenant == null)
        {
            return new TenantAccessContext();
        }

        return new TenantAccessContext
        {
            Tenant = dbTenant,
            Package = await LoadPackageIfNeededAsync(dbTenant),
        };
    }

    /// <summary>
    /// 仅在已过 <c>ExpireTime</c> 且租户未显式设置过渡期、且有套餐时，加载套餐以取默认过渡期天数。
    /// <para>正常未过期请求零查库；仅在过期边界按需查一次套餐。</para>
    /// </summary>
    private async Task<GlobalPackageEntity?> LoadPackageIfNeededAsync(SystemTenantEntity tenant)
    {
        if (tenant.ExpireTime == null
            || tenant.ExpireTime.Value > TimerHelper.UnixTimeSeconds())
        {
            return null;
        }

        if (tenant.GracePeriodDays.HasValue || !tenant.PackageId.HasValue)
        {
            return null;
        }

        return await _systemPackageRepository.GetFirstAsync(u => u.Id == tenant.PackageId.Value);
    }

    private sealed class TenantAccessContext
    {
        public SystemTenantEntity? Tenant { get; set; }

        public GlobalPackageEntity? Package { get; set; }
    }
}