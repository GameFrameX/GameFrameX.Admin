// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The App Store server notification history record, including the signed notification payload and the result of the server’s first send attempt.
/// <para>App Store 服务器通知历史记录项，包括签名的通知负载和服务器首次发送尝试的结果。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/notificationhistoryresponseitem</para>
/// </summary>
public class NotificationHistoryResponseItem
{
    /// <summary>
    /// A cryptographically signed payload, in JSON Web Signature (JWS) format, containing the response body for a version 2 notification.
    /// <para>JSON Web Signature (JWS) 格式的加密签名负载，包含版本 2 通知的响应体。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/signedpayload</para>
    /// </summary>
    public string? SignedPayload { get; set; }

    /// <summary>
    /// An array of information the App Store server records for its attempts to send a notification to your server. The maximum number of entries in the array is six.
    /// <para>App Store 服务器记录的向您的服务器发送通知的尝试信息的数组。数组中的最大条目数为 6。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/sendattemptitem</para>
    /// </summary>
    public List<SendAttemptItem>? SendAttempts { get; set; }
}