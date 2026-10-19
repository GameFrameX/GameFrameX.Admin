// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The payload data that contains an external purchase token.
/// <para>包含外部购买令牌的负载数据。</para>
/// <para>https://developer.apple.com/documentation/appstoreservernotifications/externalpurchasetoken</para>
/// </summary>
public class ExternalPurchaseToken
{
    /// <summary>
    /// The field of an external purchase token that uniquely identifies the token.
    /// <para>唯一标识令牌的外部购买令牌字段。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/externalpurchaseid</para>
    /// </summary>
    public string ExternalPurchaseId { get; set; } = null!;

    /// <summary>
    /// The field of an external purchase token that contains the UNIX date, in milliseconds, when the system created the token.
    /// <para>包含系统创建令牌时的 UNIX 日期（以毫秒为单位）的外部购买令牌字段。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/tokencreationdate</para>
    /// </summary>
    public long? TokenCreationDate { get; set; }

    /// <summary>
    /// The unique identifier of an app in the App Store.
    /// <para>App Store 中应用的唯一标识符。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/appappleid</para>
    /// </summary>
    public long? AppAppleId { get; set; }

    /// <summary>
    /// The bundle identifier of an app.
    /// <para>应用的 Bundle ID。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/bundleid</para>
    /// </summary>
    public string BundleId { get; set; } = null!;
}