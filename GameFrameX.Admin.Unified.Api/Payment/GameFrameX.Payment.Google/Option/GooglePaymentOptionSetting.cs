// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

using GameFrameX.Payment.Abstract;

namespace GameFrameX.Payment.Google.Option;

/// <summary>
/// Google Play 支付配置选项
/// </summary>
[PaymentChannel("google", "Google Play")]
public class GooglePaymentOptionSetting : IPaymentOptionSetting
{
    /// <summary>
    /// Google Play 应用包名
    /// </summary>
    [PaymentField(Label = "应用包名", Required = true)]
    public string GooglePaymentPackageName { get; set; } = string.Empty;

    /// <summary>
    /// Google Cloud 服务账号密钥 JSON 内容（运行时注入，凭证禁止入库）。
    /// 填入 Google Cloud 服务账号 JSON 全文；缺失时 <see cref="GameFrameX.Payment.Google.GooglePlayApiManager"/> 初始化 fail-closed。
    /// </summary>
    [PaymentField(Label = "服务账号密钥 JSON", Multiline = true, Sensitive = true, Help = "Google Cloud 服务账号 JSON 内容（运行时注入，禁止提交凭证文件）")]
    public string GooglePaymentServiceAccountKey { get; set; } = string.Empty;

    /// <summary>
    /// Google Pub/Sub 推送订阅配置的 OIDC audience，必须与 Pub/Sub push endpoint 的 audience 完全一致。
    /// </summary>
    [PaymentField(Label = "Pub/Sub Push Audience", Required = true, Help = "Google Pub/Sub authenticated push 配置的 audience")]
    public string GooglePaymentPubSubPushAudience { get; set; } = string.Empty;

    /// <summary>
    /// Google Pub/Sub 推送订阅用于签发 OIDC JWT 的服务账号邮箱。
    /// </summary>
    [PaymentField(Label = "Pub/Sub Push 服务账号", Required = true, Help = "Pub/Sub push authentication 使用的 service account email")]
    public string GooglePaymentPubSubPushServiceAccountEmail { get; set; } = string.Empty;

    /// <summary>
    /// 是否为沙盒环境
    /// </summary>
    [PaymentField(Label = "沙盒环境")]
    public bool GooglePaymentIsSandbox { get; set; } = true;

    /// <summary>
    /// API 超时时间（秒）
    /// </summary>
    [PaymentField(Label = "API 超时（秒）")]
    public int GooglePaymentTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// 是否启用详细日志
    /// </summary>
    [PaymentField(Label = "启用详细日志")]
    public bool GooglePaymentEnableDetailedLogging { get; set; } = false;
}