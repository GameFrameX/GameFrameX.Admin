namespace GameFrameX.Client.Api.GameUserPay.Dto;

/// <summary>
/// 支付验真响应（C11 支付域 Verify；Rcode：0=到账 / 7=pending 可疑 / 60=订单不存在）
/// </summary>
public class GameUserPayVerifyResponse
{
    /// <summary>
    /// 业务结果码（0=到账 / 7=pending 可疑 / 60=订单不存在）
    /// </summary>
    public int Rcode { get; set; }

    /// <summary>
    /// 对外订单号
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// 渠道商品 ID
    /// </summary>
    public string ProductId { get; set; } = string.Empty;
}