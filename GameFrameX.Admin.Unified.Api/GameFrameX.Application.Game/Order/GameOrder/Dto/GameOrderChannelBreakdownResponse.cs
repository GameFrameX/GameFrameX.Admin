namespace GameFrameX.Application.Game.Order.GameOrder.Dto;

/// <summary>
/// 支付渠道汇总
/// </summary>
public class GameOrderChannelBreakdownResponse
{
    public string PayChannel { get; set; } = null!;
    public long? Amount { get; set; }
    public int? Count { get; set; }
}