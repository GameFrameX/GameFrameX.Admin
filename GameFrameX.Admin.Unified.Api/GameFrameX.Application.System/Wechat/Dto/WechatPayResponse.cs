namespace GameFrameX.Application.System.Wechat.Dto;

public class WechatPayResponse
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
    /// 附加数据
    /// </summary>
    public string Attachment { get; set; } = string.Empty;

    /// <summary>
    /// 优惠标记
    /// </summary>
    public string GoodsTag { get; set; } = string.Empty;
}