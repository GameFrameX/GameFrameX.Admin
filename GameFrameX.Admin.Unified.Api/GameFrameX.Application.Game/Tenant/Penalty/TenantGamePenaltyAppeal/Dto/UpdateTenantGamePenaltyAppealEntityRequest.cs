namespace GameFrameX.Application.Game.Tenant.Penalty.TenantGamePenaltyAppeal.Dto;

/// <summary>
/// 申诉更新输入参数。申诉为事实源，不支持直接编辑；保留此类型仅为满足通用 CRUD 基类泛型约束。
/// </summary>
public class UpdateTenantGamePenaltyAppealEntityRequest : BaseUpdateRequest
{
}