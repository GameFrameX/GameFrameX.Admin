// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text.Json.Serialization;

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The request body for notification history.
/// <para>包含通知历史记录请求体的类。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/notificationhistoryrequest</para>
/// </summary>
public class NotificationHistoryRequest
{
    /// <summary>
    /// The start date of the timespan for the requested App Store Server Notification history records. The startDate needs to precede the endDate. Choose a startDate that’s within the past 180 days from the current date.
    /// <para>请求的 App Store 服务器通知历史记录的时间跨度的开始日期。startDate 需要早于 endDate。选择一个在当前日期过去 180 天内的 startDate。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/startdate</para>
    /// </summary>
    [JsonPropertyName("startDate")]
    public long? StartDate { get; set; }

    /// <summary>
    /// The end date of the timespan for the requested App Store Server Notification history records. Choose an endDate that’s later than the startDate. If you choose an endDate in the future, the endpoint automatically uses the current date as the endDate.
    /// <para>请求的 App Store 服务器通知历史记录的时间跨度的结束日期。选择一个晚于 startDate 的 endDate。如果您选择将来的 endDate，端点会自动使用当前日期作为 endDate。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/enddate</para>
    /// </summary>
    [JsonPropertyName("endDate")]
    public long? EndDate { get; set; }

    /// <summary>
    /// A notification type. Provide this field to limit the notification history records to those with this one notification type. For a list of notifications types, see notificationType.
    /// Include either the transactionId or the notificationType in your query, but not both.
    /// <para>通知类型。提供此字段以将通知历史记录限制为具有此一种通知类型的记录。有关通知类型的列表，请参阅 notificationType。在查询中包含 transactionId 或 notificationType，但不能同时包含两者。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/notificationtype</para>
    /// </summary>
    [JsonPropertyName("notificationType")]
    public NotificationTypeV2? NotificationType { get; set; }

    /// <summary>
    /// A notification subtype. Provide this field to limit the notification history records to those with this one notification subtype. For a list of subtypes, see subtype. If you specify a notificationSubtype, you need to also specify its related notificationType.
    /// <para>通知子类型。提供此字段以将通知历史记录限制为具有此一种通知子类型的记录。有关子类型的列表，请参阅 subtype。如果您指定了 notificationSubtype，则还需要指定其相关的 notificationType。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/notificationsubtype</para>
    /// </summary>
    [JsonPropertyName("notificationSubtype")]
    public Subtype? NotificationSubtype { get; set; }

    /// <summary>
    /// The transaction identifier, which may be an original transaction identifier, of any transaction belonging to the customer. Provide this field to limit the notification history request to this one customer.
    /// Include either the transactionId or the notificationType in your query, but not both.
    /// <para>属于客户的任何交易的交易标识符，可以是原始交易标识符。提供此字段以将通知历史记录请求限制为此一位客户。在查询中包含 transactionId 或 notificationType，但不能同时包含两者。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/transactionid</para>
    /// </summary>
    [JsonPropertyName("transactionId")]
    public string? TransactionId { get; set; }

    /// <summary>
    /// A Boolean value you set to true to request only the notifications that haven’t reached your server successfully. The response also includes notifications that the App Store server is currently retrying to send to your server.
    /// <para>一个布尔值，设置为 true 时仅请求未成功到达您的服务器的通知。响应还包括 App Store 服务器当前正在重试发送到您的服务器的通知。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/onlyfailures</para>
    /// </summary>
    [JsonPropertyName("onlyFailures")]
    public bool? OnlyFailures { get; set; }
}