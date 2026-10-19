// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// A decoded payload containing the version 2 notification data.
/// <para>包含版本 2 通知数据的解码负载。</para>
/// <para>https://developer.apple.com/documentation/appstoreservernotifications/responsebodyv2decodedpayload</para>
/// </summary>
public class ResponseBodyV2DecodedPayload : DecodedSignedData
{
    /// <summary>
    /// The in-app purchase event for which the App Store sends this version 2 notification.
    /// <para>App Store 发送此版本 2 通知的应用内购买事件。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/notificationtype</para>
    /// </summary>
    public NotificationTypeV2? NotificationType { get; set; }

    /// <summary>
    /// Additional information that identifies the notification event. The subtype field is present only for specific version 2 notifications.
    /// <para>标识通知事件的其他信息。subtype 字段仅出现在特定的版本 2 通知中。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/subtype</para>
    /// </summary>
    public Subtype? Subtype { get; set; }

    /// <summary>
    /// A unique identifier for the notification.
    /// <para>通知的唯一标识符。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/notificationuuid</para>
    /// </summary>
    public string? NotificationUUID { get; set; }

    /// <summary>
    /// The object that contains the app metadata and signed renewal and transaction information.
    /// The data, summary, and externalPurchaseToken fields are mutually exclusive. The payload contains only one of these fields.
    /// <para>包含应用元数据以及签名续期和交易信息的对象。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/data</para>
    /// </summary>
    public Data? Data { get; set; }

    /// <summary>
    /// A string that indicates the notification’s App Store Server Notifications version number.
    /// <para>指示通知版本的字符串。值为 2.0。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/version</para>
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// The summary data that appears when the App Store server completes your request to extend a subscription renewal date for eligible subscribers.
    /// The data, summary, and externalPurchaseToken fields are mutually exclusive. The payload contains only one of these fields.
    /// <para>App Store 发送续期扩展通知时出现的摘要数据。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/summary</para>
    /// </summary>
    public Summary? Summary { get; set; }

    /// <summary>
    /// This field appears when the notificationType is EXTERNAL_PURCHASE_TOKEN.
    /// The data, summary, and externalPurchaseToken fields are mutually exclusive. The payload contains only one of these fields.
    /// <para>对应于相关外部购买的外部购买令牌。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/externalpurchasetoken</para>
    /// </summary>
    public ExternalPurchaseToken? ExternalPurchaseToken { get; set; }

    /// <summary>
    /// The object that contains the app metadata and signed app transaction information.
    /// This field appears when the notificationType is RESCIND_CONSENT.
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/appdata</para>
    /// </summary>
    public AppData? AppData { get; set; }
}