// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text.Json.Serialization;

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The type of in-app purchase products you can offer in your app.
/// <para>您可以在应用中提供的应用内购买产品类型。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/type</para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Type
{
    /// <summary>
    /// An auto-renewable subscription.
    /// <para>自动续期订阅。</para>
    /// </summary>
    [JsonStringEnumMemberName("Auto-Renewable Subscription")]
    AutoRenewableSubscription,

    /// <summary>
    /// A non-consumable in-app purchase.
    /// <para>非消耗型应用内购买。</para>
    /// </summary>
    [JsonStringEnumMemberName("Non-Consumable")]
    NonConsumable,

    /// <summary>
    /// A consumable in-app purchase.
    /// <para>消耗型应用内购买。</para>
    /// </summary>
    [JsonStringEnumMemberName("Consumable")]
    Consumable,

    /// <summary>
    /// A non-renewing subscription.
    /// <para>非续期订阅。</para>
    /// </summary>
    [JsonStringEnumMemberName("Non-Renewing Subscription")]
    NonRenewingSubscription,
}