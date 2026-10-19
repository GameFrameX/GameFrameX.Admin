namespace GameFrameX.Application.Game.Tenant.Advertisement.TenantGameAdvertisementRewardEntity.Dto;

/// <summary>
/// 广告奖励发放账本更新输入参数。仅满足 GenericCrudService 的 TUpdateRequest 泛型约束；
/// UpdateAsync 端点已由 OnUpdatingAsync 抛错关闭（账本为事实数据，不支持管理后台修改）。
/// </summary>
public class UpdateTenantGameAdvertisementRewardEntityRequest : BaseUpdateRequest
{
}
