using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GameFrameX.Core.BaseInputOutput;

namespace GameFrameX.Client.Api.Integrations.Payment.Dto;

/// <summary>
///     客户端商品订单创建输入参数
/// </summary>
public class CreateTenantGameOrderEntityRequest : BaseAddRequest
{
    /// <summary>
    /// 账号ID
    /// </summary>
    public string AccountId { get; set; } = null!;

    /// <summary>
    /// 角色ID
    /// </summary>
    [Required(ErrorMessage = "角色ID")]
    public long RoleId { get; set; }

    /// <summary>
    /// 区服
    /// </summary>
    public long AreaId { get; set; }

    /// <summary>
    /// 支付渠道,由客户端传入
    /// </summary>
    [Required(ErrorMessage = "支付渠道不能为空")]
    public string PayChannel { get; set; } = null!;

    /// <summary>
    /// 平台,由客户端传入
    /// </summary>
    [Required(ErrorMessage = "平台不能为空")]
    public string Platform { get; set; } = null!;

    /// <summary>
    /// 客户端所属渠道
    /// </summary>
    [Required(ErrorMessage = "客户端所属渠道")]
    public string Channel { get; set; } = null!;

    /// <summary>
    /// 客户端子渠道
    /// </summary>
    public string SubChannel { get; set; } = null!;

    /// <summary>
    /// 商品,由客户端传入
    /// </summary>
    [Required(ErrorMessage = "商品不能为空")]
    public long Sku { get; set; }

    /// <summary>
    /// 货币代码，三位大写的货币代码,
    /// https://developers.xsolla.com/zh/doc/pay-station/references/supported-currencies/
    /// </summary>
    [DefaultValue("US")]
    public string Country { get; set; } = null!;

    /// <summary>
    /// 客户端IP
    /// </summary>
    [Required(ErrorMessage = "客户端IP")]
    public string ClientIp { get; set; } = null!;

    /// <summary>
    /// 支付调起场景（微信支付渠道专用）：jsapi（小程序/公众号，需配合 OpenId）/ app（原生 APP，缺省）/ native（Web 商城扫码）。
    /// 其它渠道忽略该字段。
    /// </summary>
    public string? Scene { get; set; }

    /// <summary>
    /// 用户在该应用下的微信 OpenId（微信支付 JSAPI 场景必填，用于拉起支付；其它场景忽略）
    /// </summary>
    public string? OpenId { get; set; }
}