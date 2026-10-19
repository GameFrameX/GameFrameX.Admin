namespace GameFrameX.Application.Game.Order.GameOrderFulfillment.Dto;

/// <summary>
/// 权益发货账本更新输入参数（C120）。仅满足 GenericCrudService 的 TUpdateRequest 泛型约束；
/// UpdateAsync 端点已由 OnUpdatingAsync 抛错关闭（账本为事实数据，不支持修改）。
/// </summary>
public class UpdateTenantGameOrderFulfillmentEntityRequest : BaseUpdateRequest
{
}