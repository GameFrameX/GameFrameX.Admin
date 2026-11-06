using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Entity.Business.Game;

namespace GameFrameX.Application.CommerceCenter.GameOrder.Dto;

/// <summary>
/// 游戏支付订单输出参数
/// </summary>
public class GameOrderResponse : GameOrderEntity, IBasePageResponse<GameOrderEntity>
{
}