using GameFrameX.Core.Service.TenantPackage.Dto;

namespace GameFrameX.Core.Service.TenantPackage;

/// <summary>
/// 租户有效配额服务
/// </summary>
public class TenantQuotaService
{
    private readonly TenantQuotaHelper _tenantQuotaHelper;

    public TenantQuotaService(TenantQuotaHelper tenantQuotaHelper)
    {
        _tenantQuotaHelper = tenantQuotaHelper;
    }

    /// <summary>
    /// 获取租户最终生效配额
    /// </summary>
    public Task<EffectiveQuotaResponse> GetEffectiveQuotaAsync(long tenantId)
    {
        return _tenantQuotaHelper.GetEffectiveQuotaAsync(tenantId);
    }
}