// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using GameFrameX.Payment.Abstract;

namespace GameFrameX.Payment.AliPay;

/// <summary>
/// 支付宝支付配置
/// </summary>
[PaymentChannel("alipay", "AliPay")]
public class AliPayOptionSetting : IPaymentOptionSetting
{
    /// <summary>
    /// 应用ID
    /// </summary>
    [PaymentField(Label = "应用 ID", Required = true)]
    public string AliPayAppId { get; set; } = null!;

    /// <summary>
    /// 商户私钥
    /// </summary>
    [PaymentField(Label = "商户私钥", Multiline = true, Sensitive = true)]
    public string AliPayMerchantPrivateKey { get; set; } = null!;

    /// <summary>
    /// 支付宝公钥
    /// </summary>
    [PaymentField(Label = "支付宝公钥", Multiline = true, Sensitive = true)]
    public string AliPayPublicKey { get; set; } = null!;

    /// <summary>
    /// 支付宝网关
    /// </summary>
    [PaymentField(Label = "网关地址")]
    public string AliPayGatewayUrl { get; set; } = "https://openapi.alipay.com/gateway.do";

    /// <summary>
    /// 异步通知地址
    /// </summary>
    [PaymentField(Label = "异步通知地址")]
    public string AliPayNotifyUrl { get; set; } = null!;

    /// <summary>
    /// 签名类型 (RSA2)
    /// </summary>
    [PaymentField(Label = "签名类型")]
    public string AliPaySignType { get; set; } = "RSA2";

    /// <summary>
    /// 格式 (json)
    /// </summary>
    [PaymentField(Label = "格式")]
    public string AliPayFormat { get; set; } = "json";

    /// <summary>
    /// 编码 (UTF-8)
    /// </summary>
    [PaymentField(Label = "编码")]
    public string AliPayCharset { get; set; } = "UTF-8";

    /// <summary>
    /// 销售产品码，与支付宝签约的产品码名称。
    /// 注：目前仅支持FAST_INSTANT_TRADE_PAY
    /// </summary>
    [PaymentField(Label = "销售产品码")]
    public string AliPayProductCode { get; set; } = "QUICK_MSECURITY_PAY";


    /// <summary>
    /// 支付成功客户端跳转url（由管理后台配置提供）
    /// </summary>
    [PaymentField(Label = "支付成功跳转 URL")]
    public string AliPayReturnUrl { get; set; } = null!;
}