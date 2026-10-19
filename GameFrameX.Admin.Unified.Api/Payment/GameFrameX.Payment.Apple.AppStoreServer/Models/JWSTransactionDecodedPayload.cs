// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// A decoded payload containing transaction information.
/// <para>包含交易信息的解码负载。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/jwstransactiondecodedpayload</para>
/// </summary>
public class JWSTransactionDecodedPayload : DecodedSignedData
{
    /// <summary>
    /// The original transaction identifier of a purchase.
    /// <para>购买的原始交易标识符。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/originaltransactionid</para>
    /// </summary>
    public string OriginalTransactionId { get; set; } = null!;

    /// <summary>
    /// The unique identifier for a transaction such as an in-app purchase, restored in-app purchase, or subscription renewal.
    /// <para>交易的唯一标识符，例如应用内购买、恢复的应用内购买或订阅续期。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/transactionid</para>
    /// </summary>
    public string TransactionId { get; set; } = null!;

    /// <summary>
    /// The unique identifier of subscription-purchase events across devices, including renewals.
    /// <para>跨设备的订阅购买事件（包括续期）的唯一标识符。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/weborderlineitemid</para>
    /// </summary>
    public string WebOrderLineItemId { get; set; } = null!;

    /// <summary>
    /// The bundle identifier of an app.
    /// <para>应用的 Bundle ID。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/bundleid</para>
    /// </summary>
    public string BundleId { get; set; } = null!;

    /// <summary>
    /// The unique identifier for the product, that you create in App Store Connect.
    /// <para>您在 App Store Connect 中创建的产品的唯一标识符。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/productid</para>
    /// </summary>
    public string ProductId { get; set; } = null!;

    /// <summary>
    /// The identifier of the subscription group that the subscription belongs to.
    /// <para>订阅所属的订阅组标识符。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/subscriptiongroupidentifier</para>
    /// </summary>
    public string SubscriptionGroupIdentifier { get; set; } = null!;

    /// <summary>
    /// The time that the App Store charged the user’s account for an in-app purchase, a restored in-app purchase, a subscription, or a subscription renewal after a lapse.
    /// <para>App Store 对应用内购买、恢复的应用内购买、订阅或中断后的订阅续期向用户账户收费的时间。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/purchasedate</para>
    /// </summary>
    public long? PurchaseDate { get; set; }

    /// <summary>
    /// The purchase date of the transaction associated with the original transaction identifier.
    /// <para>与原始交易标识符关联的交易的购买日期。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/originalpurchasedate</para>
    /// </summary>
    public long? OriginalPurchaseDate { get; set; }

    /// <summary>
    /// The UNIX time, in milliseconds, an auto-renewable subscription expires or renews.
    /// <para>自动续期订阅过期或续期的 UNIX 时间（以毫秒为单位）。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/expiresdate</para>
    /// </summary>
    public long? ExpiresDate { get; set; }

    /// <summary>
    /// The number of consumable products purchased.
    /// <para>购买的消耗型产品的数量。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/quantity</para>
    /// </summary>
    public int? Quantity { get; set; }

    /// <summary>
    /// The type of the in-app purchase.
    /// <para>应用内购买的类型。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/type</para>
    /// </summary>
    public Type? Type { get; set; }

    /// <summary>
    /// The UUID that an app optionally generates to map a customer’s in-app purchase with its resulting App Store transaction.
    /// <para>应用可选生成的 UUID，用于将客户的应用内购买与其产生的 App Store 交易映射。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/appaccounttoken</para>
    /// </summary>
    public string AppAccountToken { get; set; } = null!;

    /// <summary>
    /// A string that describes whether the transaction was purchased by the user, or is available to them through Family Sharing.
    /// <para>描述交易是由用户购买还是通过家庭共享提供给他们的字符串。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/inappownershiptype</para>
    /// </summary>
    public InAppOwnershipType? InAppOwnershipType { get; set; }

    /// <summary>
    /// The reason that the App Store refunded the transaction or revoked it from Family Sharing.
    /// <para>App Store 退款交易或从家庭共享中撤销交易的原因。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/revocationreason</para>
    /// </summary>
    public RevocationReason? RevocationReason { get; set; }

    /// <summary>
    /// The UNIX time, in milliseconds, that Apple Support refunded a transaction.
    /// <para>Apple 支持退款交易的 UNIX 时间（以毫秒为单位）。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/revocationdate</para>
    /// </summary>
    public long? RevocationDate { get; set; }

    /// <summary>
    /// The Boolean value that indicates whether the user upgraded to another subscription.
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/isupgraded</para>
    /// </summary>
    public bool? IsUpgraded { get; set; }

    /// <summary>
    /// A value that represents the promotional offer type.
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/offertype</para>
    /// </summary>
    public OfferType? OfferType { get; set; }

    /// <summary>
    /// The identifier that contains the offer code or the promotional offer identifier.
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/offeridentifier</para>
    /// </summary>
    public string OfferIdentifier { get; set; } = null!;

    /// <summary>
    /// The server environment, either sandbox or production.
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/environment</para>
    /// </summary>
    public ApplePayEnvironment? Environment { get; set; }

    /// <summary>
    /// The three-letter code that represents the country or region associated with the App Store storefront for the purchase.
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/storefront</para>
    /// </summary>
    public string Storefront { get; set; } = null!;

    /// <summary>
    /// An Apple-defined value that uniquely identifies the App Store storefront associated with the purchase.
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/storefrontid</para>
    /// </summary>
    public string StorefrontId { get; set; } = null!;

    /// <summary>
    /// The reason for the purchase transaction, which indicates whether it’s a customer’s purchase or a renewal for an auto-renewable subscription that the system initates.
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/transactionreason</para>
    /// </summary>
    public TransactionReason? TransactionReason { get; set; }

    /// <summary>
    /// The three-letter ISO 4217 currency code for the price of the product.
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/currency</para>
    /// </summary>
    public string Currency { get; set; } = null!;

    /// <summary>
    /// The price, in milliunits, of the in-app purchase or subscription offer that you configured in App Store Connect.
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/price</para>
    /// </summary>
    public long? Price { get; set; }

    /// <summary>
    /// The payment mode you configure for the offer.
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/offerdiscounttype</para>
    /// </summary>
    public OfferDiscountType? OfferDiscountType { get; set; }

    /// <summary>
    /// The unique identifier of the app download transaction.
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/appTransactionId</para>
    /// </summary>
    public string AppTransactionId { get; set; } = null!;

    /// <summary>
    /// The duration of the offer.
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/offerPeriod</para>
    /// </summary>
    public string OfferPeriod { get; set; } = null!;

    /// <summary>
    /// The type of the refund or revocation that applies to the transaction.
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/revocationtype</para>
    /// </summary>
    public RevocationType? RevocationType { get; set; }

    /// <summary>
    /// The percentage, in milliunits, of the transaction that the App Store has refunded or revoked.
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/revocationpercentage</para>
    /// </summary>
    public long? RevocationPercentage { get; set; }
}