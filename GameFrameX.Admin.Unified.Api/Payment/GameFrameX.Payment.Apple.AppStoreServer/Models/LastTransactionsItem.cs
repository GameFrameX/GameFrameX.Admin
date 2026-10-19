// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The most recent App Store-signed transaction information and App Store-signed renewal information for an auto-renewable subscription.
/// <para>自动续期订阅的最近一次 App Store 签名的交易信息和 App Store 签名的续期信息。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/lasttransactionsitem</para>
/// </summary>
public sealed class LastTransactionsItem
{
    /// <summary>
    /// The status of the auto-renewable subscription.
    /// <para>自动续期订阅的状态。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/status</para>
    /// </summary>
    public Status? Status { get; set; }

    /// <summary>
    /// The original transaction identifier of a purchase.
    /// <para>购买的原始交易标识符。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/originaltransactionid</para>
    /// </summary>
    public string OriginalTransactionId { get; set; } = null!;

    /// <summary>
    /// Transaction information signed by the App Store, in JSON Web Signature (JWS) format.
    /// <para>App Store 签名的交易信息，采用 JSON Web Signature (JWS) 格式。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/jwstransaction</para>
    /// </summary>
    public string SignedTransactionInfo { get; set; } = null!;

    /// <summary>
    /// Subscription renewal information, signed by the App Store, in JSON Web Signature (JWS) format.
    /// <para>App Store 签名的订阅续期信息，采用 JSON Web Signature (JWS) 格式。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/jwsrenewalinfo</para>
    /// </summary>
    public string SignedRenewalInfo { get; set; } = null!;
}