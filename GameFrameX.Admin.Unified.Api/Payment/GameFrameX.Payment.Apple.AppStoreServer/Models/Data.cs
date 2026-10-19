// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The app information that the notification data contains.
/// <para>通知数据包含的应用信息。</para>
/// <para>https://developer.apple.com/documentation/appstoreservernotifications/data</para>
/// </summary>
public class Data
{
    /// <summary>
    /// The unique identifier of the app that the notification applies to.
    /// <para>通知适用的应用的唯一标识符。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/appappleid</para>
    /// </summary>
    public long? AppAppleId { get; set; }

    /// <summary>
    /// The bundle identifier of the app.
    /// <para>应用的 Bundle ID。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/bundleid</para>
    /// </summary>
    public string BundleId { get; set; } = null!;

    /// <summary>
    /// The version of the build that identifies an iteration of the bundle.
    /// <para>标识 bundle 迭代的版本号。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/bundleversion</para>
    /// </summary>
    public string BundleVersion { get; set; } = null!;

    /// <summary>
    /// The server environment that the notification applies to, either sandbox or production.
    /// <para>通知适用的服务器环境，沙盒或生产环境。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/environment</para>
    /// </summary>
    public ApplePayEnvironment? Environment { get; set; }

    /// <summary>
    /// Subscription renewal information, signed by the App Store, in JSON Web Signature (JWS) format.
    /// <para>App Store 签名的订阅续期信息，采用 JSON Web Signature (JWS) 格式。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/signedrenewalinfo</para>
    /// </summary>
    public string SignedRenewalInfo { get; set; } = null!;

    /// <summary>
    /// Transaction information, signed by the App Store, in JSON Web Signature (JWS) format.
    /// <para>App Store 签名的交易信息，采用 JSON Web Signature (JWS) 格式。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/signedtransactioninfo</para>
    /// </summary>
    public string SignedTransactionInfo { get; set; } = null!;

    /// <summary>
    /// The status of an auto-renewable subscription as of the time the App Store signs the notification.
    /// <para>App Store 签署通知时自动续期订阅的状态。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/status</para>
    /// </summary>
    public Status? Status { get; set; }

    /// <summary>
    /// The reason the customer requested the refund.
    /// <para>客户申请退款的原因。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/consumptionrequestreason</para>
    /// </summary>
    public ConsumptionRequestReason? ConsumptionRequestReason { get; set; }
}