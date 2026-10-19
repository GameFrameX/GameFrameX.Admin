// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// A response that contains the contents of the test notification sent by the App Store server and the result from your server.
/// <para>包含 App Store 服务器发送的测试通知内容以及您服务器结果的响应。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/checktestnotificationresponse</para>
/// </summary>
public class CheckTestNotificationResponse
{
    /// <summary>
    /// A cryptographically signed payload, in JSON Web Signature (JWS) format, containing the response body for a version 2 notification.
    /// <para>包含版本 2 通知响应主体的加密签名负载，采用 JSON Web 签名 (JWS) 格式。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/signedpayload</para>
    /// </summary>
    public string SignedPayload { get; set; } = null!;

    /// <summary>
    /// An array of information the App Store server records for its attempts to send the TEST notification to your server. The array may contain a maximum of six sendAttemptItem objects.
    /// <para>App Store 服务器记录的向您的服务器发送 TEST 通知的尝试信息数组。该数组最多包含六个 sendAttemptItem 对象。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/sendattemptitem</para>
    /// </summary>
    public List<SendAttemptItem> SendAttempts { get; set; } = null!;
}