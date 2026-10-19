// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// A response that contains a list of refunded transactions for a user.
/// <para>包含用户已退款交易列表的响应。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/refundhistoryresponse</para>
/// </summary>
public class RefundHistoryResponse
{
    /// <summary>
    /// A list of up to 20 JWS transactions, or an empty array if the customer hasn't received any refunds in your app. The transactions are sorted in ascending order by revocationDate.
    /// <para>最多 20 个 JWS 交易的列表，如果客户在您的应用中未收到任何退款，则为空数组。交易按 revocationDate 升序排序。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/signedtransactions</para>
    /// </summary>
    public List<string>? SignedTransactions { get; set; }

    /// <summary>
    /// A token you use in a query to request the next set of transactions for the customer.
    /// <para>您在查询中使用的令牌，用于请求客户的下一组交易。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/revision</para>
    /// </summary>
    public string? Revision { get; set; }

    /// <summary>
    /// A Boolean value indicating whether the App Store has more transaction data.
    /// <para>一个布尔值，指示 App Store 是否有更多交易数据。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/hasmore</para>
    /// </summary>
    public bool? HasMore { get; set; }
}