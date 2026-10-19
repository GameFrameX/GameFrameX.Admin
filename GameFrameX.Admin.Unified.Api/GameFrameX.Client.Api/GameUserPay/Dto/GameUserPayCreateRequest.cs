namespace GameFrameX.Client.Api.GameUserPay.Dto;

/// <summary>
/// 支付下单请求（C11 支付域 Create）
/// </summary>
public class GameUserPayCreateRequest
{
    /// <summary>
    /// 玩家 id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 商品 SKU
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// 支付渠道（旧协议渠道号：100=Apple / 7=Google / 66=Amazon）
    /// </summary>
    public int PayChannel { get; set; }

    /// <summary>
    /// 客户端平台
    /// </summary>
    public int Platform { get; set; }

    /// <summary>
    /// 客户端渠道
    /// </summary>
    public int? Channel { get; set; }

    /// <summary>
    /// 国家/地区代码
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// 金额（分）
    /// </summary>
    public long? Amount { get; set; }

    /// <summary>
    /// 客户端 IP
    /// </summary>
    public string ClientIp { get; set; } = string.Empty;
}