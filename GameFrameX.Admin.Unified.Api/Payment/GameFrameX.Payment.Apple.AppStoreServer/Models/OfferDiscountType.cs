// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text.Json.Serialization;

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The payment mode for a discount offer on an In-App Purchase.
/// <para>应用内购买折扣优惠的支付模式。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/offerdiscounttype</para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OfferDiscountType
{
    /// <summary>
    /// A payment mode of a product discount that indicates a free trial.
    /// <para>指示免费试用的产品折扣支付模式。</para>
    /// </summary>
    FREE_TRIAL,

    /// <summary>
    /// A payment mode of a product discount that customers pay over a single or multiple billing periods.
    /// <para>客户在一个或多个计费周期内支付的产品折扣支付模式。</para>
    /// </summary>
    PAY_AS_YOU_GO,

    /// <summary>
    /// A payment mode of a product discount that customers pay up front.
    /// <para>客户预付的产品折扣支付模式。</para>
    /// </summary>
    PAY_UP_FRONT,

    /// <summary>
    /// A payment mode of a product discount that customers pay once.
    /// <para>客户一次性支付的产品折扣支付模式。</para>
    /// </summary>
    ONE_TIME,
}