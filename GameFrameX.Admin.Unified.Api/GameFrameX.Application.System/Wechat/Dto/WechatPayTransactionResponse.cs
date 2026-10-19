namespace GameFrameX.Application.System.Wechat.Dto;

/// <summary>
/// 微信支付统一下单结果（商户直连）
/// </summary>
public class WechatPayTransactionResponse
{
    /// <summary>
    /// 预支付交易会话标识（prepay_id），用于客户端调起支付
    /// </summary>
    public string? PrepayId { get; set; }

    /// <summary>
    /// 商户订单号（本地生成的商户侧唯一单号）
    /// </summary>
    public string OutTradeNumber { get; set; } = null!;
}