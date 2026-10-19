// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

using GameFrameX.Payment.Abstract;

namespace GameFrameX.Payment.Xsolla.Option;

/// <summary>
/// 全局配置信息
/// </summary>
[PaymentChannel("xsolla", "Xsolla")]
public sealed class XsollaPayOptionSetting : IPaymentOptionSetting
{
    /// <summary>
    /// 商户ID, 账号ID
    /// </summary>
    [PaymentField(Label = "商户 ID", Required = true)]
    public int XsollaPaymentMerchantId { get; set; }

    /// <summary>
    /// 项目ID， 从 https://publisher.xsolla.com/{商户ID}/projects 获取
    /// </summary>
    [PaymentField(Label = "项目 ID", Required = true)]
    public int XsollaPaymentProjectId { get; set; }

    /// <summary>
    /// API密钥, 从 https://publisher.xsolla.com/{商户ID}/settings/api_key 获取
    /// </summary>
    [PaymentField(Label = "API 密钥", Sensitive = true)]
    public string XsollaPaymentApiKey { get; set; } = string.Empty;

    /// <summary>
    /// WebHook密钥
    /// </summary>
    [PaymentField(Label = "WebHook 密钥", Sensitive = true)]
    public string XsollaPaymentWebHookSecretKey { get; set; } = string.Empty;

    /// <summary>
    /// 登录项目ID，从 https://publisher.xsolla.com/{商户ID}/projects/{项目ID}/login  获取
    /// 从项目的登录管理器中获取
    /// </summary>
    [PaymentField(Label = "登录项目 ID")]
    public string XsollaPaymentLoginProjectId { get; set; } = string.Empty;

    /// <summary>
    /// 登录管理器中创建的Oauth2.0 ID
    /// </summary>
    [PaymentField(Label = "OAuth2.0 Client ID")]
    public string XsollaPaymentClientId { get; set; } = string.Empty;

    /// <summary>
    /// 登录管理器中创建的Oauth2.0 秘钥
    /// </summary>
    [PaymentField(Label = "OAuth2.0 Client Secret", Sensitive = true)]
    public string XsollaPaymentClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// 当使用沙盒支付的时候，必须要设置邮件地址。如果应用是非正式环境，则不需要设置。
    /// </summary>
    [PaymentField(Label = "沙盒测试邮箱", Help = "沙盒支付必填")]
    public string XsollaPaymentEmail { get; set; } = string.Empty;
}