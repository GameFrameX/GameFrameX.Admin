namespace GameFrameX.Application.System.Wechat.Dto;

/// <summary>
/// 微信支付订单详情输出参数
/// </summary>
public class SystemWechatPayResponse
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 商户号
    /// </summary>
    public string MerchantId { get; set; } = string.Empty;

    /// <summary>
    /// 应用Id
    /// </summary>
    public string AppId { get; set; } = string.Empty;

    /// <summary>
    /// 商户订单号
    /// </summary>
    public string OutTradeNumber { get; set; } = string.Empty;

    /// <summary>
    /// 微信支付订单号
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// 交易类型
    /// </summary>
    public string? TradeType { get; set; }

    /// <summary>
    /// 交易状态
    /// </summary>
    public string? TradeState { get; set; }

    /// <summary>
    /// 交易状态描述
    /// </summary>
    public string? TradeStateDescription { get; set; }

    /// <summary>
    /// 付款银行
    /// </summary>
    public string? BankType { get; set; }

    /// <summary>
    /// 订单总金额（分）
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 用户支付金额（分）
    /// </summary>
    public int? PayerTotal { get; set; }

    /// <summary>
    /// 支付完成时间
    /// </summary>
    public long? SuccessTime { get; set; }

    /// <summary>
    /// 订单失效时间
    /// </summary>
    public long? ExpireTime { get; set; }

    /// <summary>
    /// 商品描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 支付场景
    /// </summary>
    public string? Scene { get; set; }

    /// <summary>
    /// 附加数据
    /// </summary>
    public string? Attachment { get; set; }

    /// <summary>
    /// 订单优惠标记
    /// </summary>
    public string? GoodsTag { get; set; }

    /// <summary>
    /// 结算信息
    /// </summary>
    public string? Settlement { get; set; }

    /// <summary>
    /// 回调地址
    /// </summary>
    public string? NotifyUrl { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 用户OpenId
    /// </summary>
    public string? OpenId { get; set; }

    /// <summary>
    /// 子商户号
    /// </summary>
    public string? SubMerchantId { get; set; }

    /// <summary>
    /// 子应用Id
    /// </summary>
    public string? SubAppId { get; set; }

    /// <summary>
    /// 子用户OpenId
    /// </summary>
    public string? SubOpenId { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// 行版本号（乐观锁）
    /// </summary>
    public long? RowVersion { get; set; }
}