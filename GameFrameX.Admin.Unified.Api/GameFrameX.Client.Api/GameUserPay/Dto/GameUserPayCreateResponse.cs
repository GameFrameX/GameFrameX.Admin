namespace GameFrameX.Client.Api.GameUserPay.Dto;

/// <summary>
/// 支付下单响应（C11 支付域 Create；沙箱策略命中时 IsNeedPay=false 即免支付直发）
/// </summary>
public class GameUserPayCreateResponse
{
    /// <summary>
    /// 对外订单号（服务端生成）
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// 是否需要支付（沙箱直发 = false）
    /// </summary>
    public bool IsNeedPay { get; set; }

    /// <summary>
    /// 金额（分；回显下单金额）
    /// </summary>
    public long? Amount { get; set; }

    /// <summary>
    /// 商品 SKU（回显）
    /// </summary>
    public string Sku { get; set; } = string.Empty;
}