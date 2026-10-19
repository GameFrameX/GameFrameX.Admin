// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// A response that indicates whether an individual renewal-date extension succeeded, and related details.
/// <para>指示单个续期日期延长是否成功以及相关详细信息的响应。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/extendrenewaldateresponse</para>
/// </summary>
public class ExtendRenewalDateResponse
{
    /// <summary>
    /// The original transaction identifier of a purchase.
    /// <para>购买的原始交易标识符。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/originaltransactionid</para>
    /// </summary>
    public string OriginalTransactionId { get; set; } = null!;

    /// <summary>
    /// The unique identifier of subscription-purchase events across devices, including renewals.
    /// <para>跨设备的订阅购买事件（包括续期）的唯一标识符。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/weborderlineitemid</para>
    /// </summary>
    public string WebOrderLineItemId { get; set; } = null!;

    /// <summary>
    /// A Boolean value that indicates whether the subscription-renewal-date extension succeeded.
    /// <para>指示订阅续期日期延长是否成功的布尔值。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/success</para>
    /// <para>命名豁免（GFX-707）：Apple App Store Server API 官方契约字段 <c>success</c>，保留原生命名，不改 IsSuccess。</para>
    /// </summary>
    public bool? Success { get; set; }

    /// <summary>
    /// The new subscription expiration date for a subscription-renewal extension.
    /// <para>订阅续期延长后的新订阅过期日期。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/effectivedate</para>
    /// </summary>
    public long? EffectiveDate { get; set; }
}