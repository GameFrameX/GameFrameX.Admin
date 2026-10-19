// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The request body containing consumption information.
/// <para>包含消费信息的请求体。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/consumptionrequestv1</para>
/// </summary>
[Obsolete("Use ConsumptionRequest instead.")]
public class ConsumptionRequestV1
{
    /// <summary>
    /// A Boolean value that indicates whether the customer consented to provide consumption data to the App Store.
    /// <para>指示客户是否同意向 App Store 提供消费数据的布尔值。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/customerconsented</para>
    /// </summary>
    public bool? CustomerConsented { get; set; }

    /// <summary>
    /// A value that indicates the extent to which the customer consumed the in-app purchase.
    /// <para>指示客户消费应用内购买程度的值。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/consumptionstatus</para>
    /// </summary>
    public ConsumptionStatus? ConsumptionStatus { get; set; }

    /// <summary>
    /// A value that indicates the platform on which the customer consumed the in-app purchase.
    /// <para>指示客户消费应用内购买的平台的值。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/platform</para>
    /// </summary>
    public Platform? Platform { get; set; }

    /// <summary>
    /// A Boolean value that indicates whether you provided, prior to its purchase, a free sample or trial of the content, or information about its functionality.
    /// <para>指示您是否在购买前提供了内容的免费样本或试用版，或有关其功能的信息的布尔值。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/samplecontentprovided</para>
    /// </summary>
    public bool? SampleContentProvided { get; set; }

    /// <summary>
    /// A value that indicates whether the app successfully delivered an in-app purchase that works properly.
    /// <para>指示应用是否成功交付了正常工作的应用内购买的值。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/deliverystatus</para>
    /// </summary>
    public DeliveryStatusV1? DeliveryStatus { get; set; }

    /// <summary>
    /// The UUID that an app optionally generates to map a customer’s in-app purchase with its resulting App Store transaction.
    /// <para>应用可选生成的 UUID，用于将客户的应用内购买与其产生的 App Store 交易进行映射。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/appaccounttoken</para>
    /// </summary>
    public string AppAccountToken { get; set; } = null!;

    /// <summary>
    /// The age of the customer’s account.
    /// <para>客户账户的注册年限。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/accounttenure</para>
    /// </summary>
    public AccountTenure? AccountTenure { get; set; }

    /// <summary>
    /// A value that indicates the amount of time that the customer used the app.
    /// <para>指示客户使用应用的时间量的值。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/consumptionrequest</para>
    /// </summary>
    public PlayTime? PlayTime { get; set; }

    /// <summary>
    /// A value that indicates the total amount, in USD, of refunds the customer has received, in your app, across all platforms.
    /// <para>指示客户在所有平台上在您的应用中收到的退款总额（以美元为单位）的值。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/lifetimedollarsrefunded</para>
    /// </summary>
    public LifetimeDollarsRefunded? LifetimeDollarsRefunded { get; set; }

    /// <summary>
    /// A value that indicates the total amount, in USD, of in-app purchases the customer has made in your app, across all platforms.
    /// <para>指示客户在所有平台上在您的应用中进行的应用内购买总额（以美元为单位）的值。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/lifetimedollarspurchased</para>
    /// </summary>
    public LifetimeDollarsPurchased? LifetimeDollarsPurchased { get; set; }

    /// <summary>
    /// The status of the customer’s account.
    /// <para>客户账户的状态。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/userstatus</para>
    /// </summary>
    public UserStatus? UserStatus { get; set; }

    /// <summary>
    /// A value that indicates your preference, based on your operational logic, as to whether Apple should grant the refund.
    /// <para>指示您根据您的运营逻辑，对 Apple 是否应批准退款的偏好的值。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/refundpreference</para>
    /// </summary>
    public RefundPreferenceV1? RefundPreference { get; set; }
}