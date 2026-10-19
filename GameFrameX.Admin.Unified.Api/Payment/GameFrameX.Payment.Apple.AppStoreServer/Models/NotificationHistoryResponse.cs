// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// A response that contains the App Store Server Notifications history for your app.
/// <para>包含应用的 App Store 服务器通知历史记录的响应。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/notificationhistoryresponse</para>
/// </summary>
public class NotificationHistoryResponse
{
    /// <summary>
    /// A pagination token that you return to the endpoint on a subsequent call to receive the next set of results.
    /// <para>分页标记，您在后续调用中将其返回给端点以接收下一组结果。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/paginationtoken</para>
    /// </summary>
    public string? PaginationToken { get; set; }

    /// <summary>
    /// A Boolean value indicating whether the App Store has more transaction data.
    /// <para>一个布尔值，指示 App Store 是否有更多交易数据。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/hasmore</para>
    /// </summary>
    public bool? HasMore { get; set; }

    /// <summary>
    /// An array of App Store server notification history records.
    /// <para>App Store 服务器通知历史记录数组。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/notificationhistoryresponseitem</para>
    /// </summary>
    public List<NotificationHistoryResponseItem>? NotificationHistory { get; set; }
}