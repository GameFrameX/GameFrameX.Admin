// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The type of offer.
/// <para>优惠类型。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/offertype</para>
/// </summary>
public enum OfferType
{
    /// <summary>
    /// An introductory offer.
    /// <para>推介优惠。</para>
    /// </summary>
    INTRODUCTORY_OFFER = 1,

    /// <summary>
    /// A promotional offer.
    /// <para>促销优惠。</para>
    /// </summary>
    PROMOTIONAL_OFFER = 2,

    /// <summary>
    /// An offer with a subscription offer code.
    /// <para>带有订阅优惠代码的优惠。</para>
    /// </summary>
    OFFER_CODE = 3,

    /// <summary>
    /// A win-back offer.
    /// <para>赢回优惠。</para>
    /// </summary>
    WIN_BACK_OFFER = 4,
}