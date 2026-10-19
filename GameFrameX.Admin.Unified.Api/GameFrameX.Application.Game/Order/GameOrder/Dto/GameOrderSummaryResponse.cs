namespace GameFrameX.Application.Game.Order.GameOrder.Dto;

/// <summary>
/// 订单统计输出
/// </summary>
public class GameOrderSummaryResponse
{
    /// <summary>
    /// 匹配订单总金额(分)
    /// </summary>
    public long TotalAmount { get; set; }

    /// <summary>
    /// 匹配订单总数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 已付款订单总金额(分)
    /// </summary>
    public long PaidAmount { get; set; }

    /// <summary>
    /// 已付款订单总数
    /// </summary>
    public int PaidCount { get; set; }

    /// <summary>
    /// 按支付渠道金额汇总
    /// </summary>
    public List<GameOrderChannelBreakdownResponse> PayChannelBreakdown { get; set; } = new();

    /// <summary>
    /// 按订单状态金额汇总
    /// </summary>
    public List<GameOrderStatusBreakdownResponse> StatusBreakdown { get; set; } = new();
}