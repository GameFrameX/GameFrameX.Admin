// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text.Json.Serialization;

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The request body that contains subscription-renewal-extension data to apply for all eligible active subscribers.
/// <para>包含用于申请所有符合条件的活跃订阅者的订阅续期延长数据的请求体。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/massextendrenewaldaterequest</para>
/// </summary>
public sealed class MassExtendRenewalDateRequest
{
    /// <summary>
    /// The number of days to extend the subscription renewal date.
    /// <para>延长订阅续期日期的天数。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/extendbydays</para>
    /// </summary>
    [JsonPropertyName("extendByDays")]
    public int? ExtendByDays { get; set; }

    /// <summary>
    /// The reason code for the subscription-renewal-date extension.
    /// <para>订阅续期日期延长的原因代码。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/extendreasoncode</para>
    /// </summary>
    [JsonPropertyName("extendReasonCode")]
    public ExtendReasonCode? ExtendReasonCode { get; set; }

    /// <summary>
    /// A string that contains a unique identifier you provide to track each subscription-renewal-date extension request.
    /// <para>包含您提供的用于跟踪每个订阅续期日期延长请求的唯一标识符的字符串。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/requestidentifier</para>
    /// </summary>
    [JsonPropertyName("requestIdentifier")]
    public string? RequestIdentifier { get; set; }

    /// <summary>
    /// A list of storefront country codes you provide to limit the storefronts for a subscription-renewal-date extension.
    /// <para>您提供的店面国家/地区代码列表，用于限制订阅续期日期延长的店面。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/storefrontcountrycodes</para>
    /// </summary>
    [JsonPropertyName("storefrontCountryCodes")]
    public List<string>? StorefrontCountryCodes { get; set; }

    /// <summary>
    /// The unique identifier for the product, that you create in App Store Connect.
    /// <para>您在 App Store Connect 中创建的产品的唯一标识符。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/productid</para>
    /// </summary>
    [JsonPropertyName("productId")]
    public string? ProductId { get; set; }
}