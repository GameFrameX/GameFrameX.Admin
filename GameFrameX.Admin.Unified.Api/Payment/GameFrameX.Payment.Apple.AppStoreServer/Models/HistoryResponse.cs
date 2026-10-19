// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// A response that contains the customer’s transaction history for an app.
/// <para>包含客户在应用中的交易历史记录的响应。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/historyresponse</para>
/// </summary>
public class HistoryResponse
{
    /// <summary>
    /// A token you use in a query to request the next set of transactions for the customer.
    /// <para>用于请求客户下一组交易的查询令牌。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/revision</para>
    /// </summary>
    public string Revision { get; set; } = null!;

    /// <summary>
    /// A Boolean value indicating whether the App Store has more transaction data.
    /// <para>指示 App Store 是否有更多交易数据的布尔值。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/hasmore</para>
    /// </summary>
    public bool? HasMore { get; set; }

    /// <summary>
    /// The bundle identifier of an app.
    /// <para>应用的 Bundle ID。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/bundleid</para>
    /// </summary>
    public string BundleId { get; set; } = null!;

    /// <summary>
    /// The unique identifier of an app in the App Store.
    /// <para>App Store 中应用的唯一标识符。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/appappleid</para>
    /// </summary>
    public long? AppAppleId { get; set; }

    /// <summary>
    /// The server environment in which you’re making the request, whether sandbox or production.
    /// <para>发出请求的服务器环境，沙盒或生产环境。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/environment</para>
    /// </summary>
    public ApplePayEnvironment? Environment { get; set; }

    /// <summary>
    /// An array of in-app purchase transactions for the customer, signed by Apple, in JSON Web Signature format.
    /// <para>客户的应用内购买交易数组，由 Apple 签名，采用 JSON Web Signature 格式。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/jwstransaction</para>
    /// </summary>
    public List<string> SignedTransactions { get; set; } = null!;
}