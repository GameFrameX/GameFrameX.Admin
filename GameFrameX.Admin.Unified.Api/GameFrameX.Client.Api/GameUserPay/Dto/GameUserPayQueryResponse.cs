namespace GameFrameX.Client.Api.GameUserPay.Dto;

/// <summary>
/// 支付订单查询响应（C11 支付域 Query；查无时全字段空值 + code 0）
/// </summary>
public class GameUserPayQueryResponse
{
    /// <summary>
    /// 对外订单号
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// 渠道商品 ID
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// 订单状态（OrderStatusEnum 值：1=Unpaid / 2=NotShipped / 3=Shipped）
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 是否已支付到账（Status &gt;= Shipped）
    /// </summary>
    public bool Paid { get; set; }

    /// <summary>
    /// 金额（分）
    /// </summary>
    public long? Amount { get; set; }
}