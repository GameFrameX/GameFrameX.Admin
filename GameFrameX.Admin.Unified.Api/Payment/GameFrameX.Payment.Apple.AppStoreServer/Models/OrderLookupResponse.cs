// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// A response that includes the order lookup status and an array of signed transactions for the in-app purchases in the order.
/// <para>包含订单查找状态和订单中应用内购买的签名交易数组的响应。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/orderlookupresponse</para>
/// </summary>
public class OrderLookupResponse
{
    /// <summary>
    /// The status that indicates whether the order ID is valid.
    /// <para>指示订单 ID 是否有效的状态。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/orderlookupstatus</para>
    /// </summary>
    public OrderLookupStatus? Status { get; set; }

    /// <summary>
    /// An array of in-app purchase transactions that are part of order, signed by Apple, in JSON Web Signature format.
    /// <para>订单中属于应用内购买交易的数组，由 Apple 签名，格式为 JSON Web Signature。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/jwstransaction</para>
    /// </summary>
    public List<string>? SignedTransactions { get; set; }
}