// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text.Json.Serialization;

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The request body that contains consumption information for an In-App Purchase.
/// <para>包含应用内购买消费信息的请求体。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/consumptionrequest</para>
/// </summary>
public class ConsumptionRequest
{
    /// <summary>
    /// A Boolean value that indicates whether the customer consented to provide consumption data to the App Store.
    /// <para>指示客户是否同意向 App Store 提供消费数据的布尔值。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/customerconsented</para>
    /// </summary>
    [JsonPropertyName("customerConsented")]
    public bool CustomerConsented { get; set; }

    /// <summary>
    /// An integer that indicates the percentage, in milliunits, of the In-App Purchase the customer consumed.
    /// <para>指示客户消费应用内购买百分比的整数（以千分比为单位）。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/consumptionpercentage</para>
    /// </summary>
    [JsonPropertyName("consumptionPercentage")]
    public int? ConsumptionPercentage { get; set; }

    /// <summary>
    /// A value that indicates whether the app successfully delivered an in-app purchase that works properly.
    /// <para>指示应用是否成功交付了正常工作的应用内购买的值。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/deliverystatus</para>
    /// </summary>
    [JsonPropertyName("deliveryStatus")]
    public DeliveryStatus DeliveryStatus { get; set; }

    /// <summary>
    /// A value that indicates your preferred outcome for the refund request.
    /// <para>指示您对退款请求的首选结果的值。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/refundpreference</para>
    /// </summary>
    [JsonPropertyName("refundPreference")]
    public RefundPreference? RefundPreference { get; set; }

    /// <summary>
    /// A Boolean value that indicates whether you provided, prior to its purchase, a free sample or trial of the content, or information about its functionality.
    /// <para>指示您是否在购买前提供了内容的免费样本或试用版，或有关其功能的信息的布尔值。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/samplecontentprovided</para>
    /// </summary>
    [JsonPropertyName("sampleContentProvided")]
    public bool SampleContentProvided { get; set; }
}