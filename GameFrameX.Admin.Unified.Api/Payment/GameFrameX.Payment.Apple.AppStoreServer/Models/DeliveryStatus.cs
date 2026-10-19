// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text.Json.Serialization;

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// A value that indicates whether the app successfully delivered an In-App Purchase that works properly.
/// <para>指示应用是否成功交付了正常工作的应用内购买项目的值。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/deliverystatus</para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DeliveryStatus
{
    /// <summary>
    /// The app delivered the consumable content and it’s working properly.
    /// <para>应用已交付消耗型内容，且工作正常。</para>
    /// </summary>
    DELIVERED = 0,

    /// <summary>
    /// The app didn’t deliver the consumable content because of a quality issue.
    /// <para>由于质量问题，应用未交付消耗型内容。</para>
    /// </summary>
    UNDELIVERED_QUALITY_ISSUE = 1,

    /// <summary>
    /// The app delivered the wrong item.
    /// <para>应用交付了错误的物品。</para>
    /// </summary>
    UNDELIVERED_WRONG_ITEM = 2,

    /// <summary>
    /// The app didn’t deliver the consumable content because of a server outage.
    /// <para>由于服务器中断，应用未交付消耗型内容。</para>
    /// </summary>
    UNDELIVERED_SERVER_OUTAGE = 3,

    /// <summary>
    /// The app didn’t deliver the consumable content for other reasons.
    /// <para>由于其他原因，应用未交付消耗型内容。</para>
    /// </summary>
    UNDELIVERED_OTHER = 5,
}