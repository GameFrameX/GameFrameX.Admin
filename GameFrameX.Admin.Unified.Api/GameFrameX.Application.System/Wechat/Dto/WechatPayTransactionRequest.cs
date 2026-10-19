namespace GameFrameX.Application.System.Wechat.Dto;

public class WechatPayTransactionRequest
{
    /// <summary>
    /// OpenId
    /// </summary>
    public string OpenId { get; set; } = string.Empty;

    /// <summary>
    /// 订单金额
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 商品描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 附加数据
    /// </summary>
    public string Attachment { get; set; } = string.Empty;

    /// <summary>
    /// 优惠标记
    /// </summary>
    public string GoodsTag { get; set; } = string.Empty;
}