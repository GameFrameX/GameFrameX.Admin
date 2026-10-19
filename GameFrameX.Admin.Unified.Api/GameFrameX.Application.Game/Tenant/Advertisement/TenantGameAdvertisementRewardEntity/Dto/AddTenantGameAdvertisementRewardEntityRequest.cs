namespace GameFrameX.Application.Game.Tenant.Advertisement.TenantGameAdvertisementRewardEntity.Dto;

/// <summary>
/// 广告奖励发放账本新增输入参数。仅满足 GenericCrudService 的 TAddRequest 泛型约束；
/// AddAsync 端点已由 OnAddingAsync 抛错关闭（账本由广告回调链路产生，不支持管理后台直接创建）。
/// </summary>
public class AddTenantGameAdvertisementRewardEntityRequest : BaseAddRequest
{
}
