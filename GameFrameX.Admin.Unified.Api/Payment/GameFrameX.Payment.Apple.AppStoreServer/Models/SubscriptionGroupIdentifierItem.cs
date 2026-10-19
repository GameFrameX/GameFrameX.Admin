// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// Information for a subscription group.
/// <para>订阅组的信息。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/subscriptiongroupidentifieritem</para>
/// </summary>
public class SubscriptionGroupIdentifierItem
{
    /// <summary>
    /// The identifier of the subscription group that the subscription belongs to.
    /// <para>订阅所属订阅组的标识符。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/subscriptiongroupidentifier</para>
    /// </summary>
    public string? SubscriptionGroupIdentifier { get; set; }

    /// <summary>
    /// A list of the latest transaction information for each unique subscription identifier in the subscription group.
    /// <para>订阅组中每个唯一订阅标识符的最新交易信息列表。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/lasttransactionsitem</para>
    /// </summary>
    public List<LastTransactionsItem>? LastTransactions { get; set; }
}