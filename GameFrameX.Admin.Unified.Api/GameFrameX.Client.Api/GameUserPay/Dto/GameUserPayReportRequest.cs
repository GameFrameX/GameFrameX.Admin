namespace GameFrameX.Client.Api.GameUserPay.Dto;

/// <summary>
/// 支付流水上报请求（C11 支付域 Report；纯记账对账，不推进订单状态，响应 data 为 null）
/// </summary>
public class GameUserPayReportRequest
{
    /// <summary>
    /// 玩家 id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 对外订单号
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// 实付金额（分）
    /// </summary>
    public long RealMoney { get; set; }

    /// <summary>
    /// 币种
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// 渠道商品 ID
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// 下单时玩家关卡
    /// </summary>
    public int? Level { get; set; }

    /// <summary>
    /// 支付渠道（旧协议渠道号：100=Apple / 7=Google / 66=Amazon）
    /// </summary>
    public int PayChannel { get; set; }

    /// <summary>
    /// 支付场景
    /// </summary>
    public string StrScene { get; set; } = string.Empty;

    /// <summary>
    /// 客户端渠道
    /// </summary>
    public int? Channel { get; set; }
}