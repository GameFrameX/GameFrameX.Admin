using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Application.Game.Order.GameOrder.Dto;

/// <summary>
/// 游戏支付订单输出参数
/// </summary>
public class TenantGameOrderEntityResponse : TenantGameOrderEntity, IBasePageResponse<TenantGameOrderEntity>
{
}