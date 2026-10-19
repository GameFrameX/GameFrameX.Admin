// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text.Json.Serialization;

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The cause of a purchase transaction, which indicates whether it’s a customer’s purchase or a renewal for an auto-renewable subscription that the system initiates.
/// <para>购买交易的原因，指示是客户购买还是系统发起的自动续期订阅的续期。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/transactionreason</para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionReason
{
    /// <summary>
    /// The customer initiated the purchase.
    /// <para>客户发起了购买。</para>
    /// </summary>
    PURCHASE,

    /// <summary>
    /// The system initiated the purchase because of an auto-renewable subscription renewal.
    /// <para>由于自动续期订阅续期，系统发起了购买。</para>
    /// </summary>
    RENEWAL,
}