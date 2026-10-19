// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using GameFrameX.Payment.Abstract;

namespace GameFrameX.Payment.Wechat;

/// <summary>
/// 微信支付配置（商户直连模式）。渠道名 wechat，声明式标注由 <see cref="PaymentChannelRegistry"/> 自动发现，
/// 敏感字段（APIv3 密钥 / 商户私钥）按 <c>[PaymentField(Sensitive = true)]</c> 走边界字段级加解密。
/// </summary>
[PaymentChannel("wechat", "微信支付")]
public class WechatPayOptionSetting : IPaymentOptionSetting
{
    /// <summary>
    /// 商户号（微信支付商户平台的商户号）
    /// </summary>
    [PaymentField(Label = "商户号", Required = true)]
    public string WechatPayMerchantId { get; set; } = null!;

    /// <summary>
    /// 应用 ID（小程序 / 公众号 / APP 的 AppId，直连模式下与商户号绑定）
    /// </summary>
    [PaymentField(Label = "应用 ID", Required = true)]
    public string WechatPayAppId { get; set; } = null!;

    /// <summary>
    /// APIv3 密钥（用于回调资源 AES-256-GCM 解密与平台证书下载）
    /// </summary>
    [PaymentField(Label = "APIv3 密钥", Required = true, Sensitive = true)]
    public string WechatPayApiV3Secret { get; set; } = null!;

    /// <summary>
    /// 商户 API 证书序列号
    /// </summary>
    [PaymentField(Label = "商户证书序列号", Required = true)]
    public string WechatPayCertificateSerialNumber { get; set; } = null!;

    /// <summary>
    /// 商户 API 证书私钥（apiclient_key.pem 内容，PEM 或裸密钥均可）
    /// </summary>
    [PaymentField(Label = "商户私钥", Required = true, Multiline = true, Sensitive = true)]
    public string WechatPayCertificatePrivateKey { get; set; } = null!;

    /// <summary>
    /// 支付结果回调通知地址。运行期由 PaymentOptionSettingProvider 自动追加租户 / 应用 / 分包路由段
    /// （公开回调端点格式），配置时只需填写站点根地址。
    /// </summary>
    [PaymentField(Label = "回调通知地址", Required = true, Help = "填写站点根地址即可，系统自动追加租户路由段")]
    public string WechatPayNotifyUrl { get; set; } = null!;
}
