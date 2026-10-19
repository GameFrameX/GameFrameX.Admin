// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text.Json.Serialization;

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The request body for transaction history.
/// <para>交易历史记录的请求体。</para>
/// </summary>
public class TransactionHistoryRequest
{
    /// <summary>
    /// An optional start date of the timespan for the transaction history records you’re requesting. The startDate must precede the endDate if you specify both dates. To be included in results, the transaction’s purchaseDate must be equal to or greater than the startDate.
    /// <para>您请求的交易历史记录时间跨度的可选开始日期。如果您指定了两个日期，startDate 必须早于 endDate。要包含在结果中，交易的 purchaseDate 必须等于或大于 startDate。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/startdate</para>
    /// </summary>
    [JsonPropertyName("startDate")]
    public long? StartDate { get; set; }

    /// <summary>
    /// An optional end date of the timespan for the transaction history records you’re requesting. Choose an endDate that’s later than the startDate if you specify both dates. Using an endDate in the future is valid. To be included in results, the transaction’s purchaseDate must be less than the endDate.
    /// <para>您请求的交易历史记录时间跨度的可选结束日期。如果您指定了两个日期，请选择晚于 startDate 的 endDate。使用未来的 endDate 是有效的。要包含在结果中，交易的 purchaseDate 必须小于 endDate。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/enddate</para>
    /// </summary>
    [JsonPropertyName("endDate")]
    public long? EndDate { get; set; }

    /// <summary>
    /// An optional filter that indicates the product identifier to include in the transaction history. Your query may specify more than one productID.
    /// <para>指示要包含在交易历史记录中的产品标识符的可选过滤器。您的查询可以指定多个 productID。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/productid</para>
    /// </summary>
    [JsonPropertyName("productIds")]
    public List<string>? ProductIds { get; set; }

    /// <summary>
    /// An optional filter that indicates the product type to include in the transaction history. Your query may specify more than one productType.
    /// <para>指示要包含在交易历史记录中的产品类型的可选过滤器。您的查询可以指定多个 productType。</para>
    /// </summary>
    [JsonPropertyName("productTypes")]
    public List<ProductType>? ProductTypes { get; set; }

    /// <summary>
    /// An optional sort order for the transaction history records. The response sorts the transaction records by their recently modified date. The default value is ASCENDING, so you receive the oldest records first.
    /// <para>交易历史记录的可选排序顺序。响应按最近修改日期对交易记录进行排序。默认值为 ASCENDING，因此您首先收到最早的记录。</para>
    /// </summary>
    [JsonPropertyName("sort")]
    public Order? Sort { get; set; }

    /// <summary>
    /// An optional filter that indicates the subscription group identifier to include in the transaction history. Your query may specify more than one subscriptionGroupIdentifier.
    /// <para>指示要包含在交易历史记录中的订阅组标识符的可选过滤器。您的查询可以指定多个 subscriptionGroupIdentifier。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/subscriptiongroupidentifier</para>
    /// </summary>
    [JsonPropertyName("subscriptionGroupIdentifiers")]
    public List<string>? SubscriptionGroupIdentifiers { get; set; }

    /// <summary>
    /// An optional filter that limits the transaction history by the in-app ownership type.
    /// <para>按应用内所有权类型限制交易历史记录的可选过滤器。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/inappownershiptype</para>
    /// </summary>
    [JsonPropertyName("inAppOwnershipType")]
    public InAppOwnershipType? InAppOwnershipType { get; set; }

    /// <summary>
    /// An optional Boolean value that indicates whether the response includes only revoked transactions when the value is true, or contains only nonrevoked transactions when the value is false. By default, the request doesn't include this parameter.
    /// <para>一个可选的布尔值，当值为 true 时，指示响应是否仅包含已撤销的交易；当值为 false 时，指示响应是否仅包含未撤销的交易。默认情况下，请求不包含此参数。</para>
    /// </summary>
    [JsonPropertyName("revoked")]
    public bool? Revoked { get; set; }
}