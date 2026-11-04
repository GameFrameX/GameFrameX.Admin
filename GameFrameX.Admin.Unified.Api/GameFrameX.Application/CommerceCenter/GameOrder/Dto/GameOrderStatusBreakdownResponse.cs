using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.CommerceCenter.GameOrder.Dto;

/// <summary>
/// 订单状态汇总
/// </summary>
public class GameOrderStatusBreakdownResponse
{
    public OrderStatusEnum Status { get; set; }
    public long? Amount { get; set; }
    public int? Count { get; set; }
}