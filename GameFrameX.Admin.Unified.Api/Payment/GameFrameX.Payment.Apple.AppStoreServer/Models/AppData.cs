// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The object that contains the app metadata and signed app transaction information.
/// <para>包含应用元数据和签名应用交易信息的对象。</para>
/// <para>https://developer.apple.com/documentation/appstoreservernotifications/appdata</para>
/// </summary>
public class AppData
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
    /// The server environment that the notification applies to, either sandbox or production.
    /// <para>通知适用的服务器环境，沙盒或生产环境。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/environment</para>
    /// </summary>
    public ApplePayEnvironment? Environment { get; set; }

    /// <summary>
    /// App transaction information signed by the App Store, in JSON Web Signature (JWS) format.
    /// <para>由 App Store 签名的应用交易信息，采用 JSON Web 签名 (JWS) 格式。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/jwsapptransaction</para>
    /// </summary>
    public string SignedAppTransactionInfo { get; set; } = null!;
}