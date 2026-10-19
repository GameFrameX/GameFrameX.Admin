// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text.Json.Serialization;

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The customer-provided reason for a refund request.
/// <para>客户提供的退款请求原因。</para>
/// <para>https://developer.apple.com/documentation/appstoreservernotifications/consumptionrequestreason</para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConsumptionRequestReason
{
    /// <summary>
    /// The customer didn’t intend to make the purchase.
    /// <para>客户无意购买。</para>
    /// </summary>
    UNINTENDED_PURCHASE,

    /// <summary>
    /// The customer didn’t receive the in-app purchase, or it didn’t work as expected.
    /// <para>客户未收到应用内购买项目，或其无法按预期工作。</para>
    /// </summary>
    FULFILLMENT_ISSUE,

    /// <summary>
    /// The customer wasn’t satisfied with the in-app purchase.
    /// <para>客户对应用内购买项目不满意。</para>
    /// </summary>
    UNSATISFIED_WITH_PURCHASE,

    /// <summary>
    /// The customer requested a refund due to a legal issue.
    /// <para>客户因法律问题申请退款。</para>
    /// </summary>
    LEGAL,

    /// <summary>
    /// The customer requested a refund for another reason.
    /// <para>客户因其他原因申请退款。</para>
    /// </summary>
    OTHER,
}