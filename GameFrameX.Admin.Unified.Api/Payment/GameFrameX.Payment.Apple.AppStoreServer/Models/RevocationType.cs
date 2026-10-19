// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text.Json.Serialization;

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The type of the refund or revocation that applies to the transaction.
/// <para>适用于交易的退款或撤销类型。</para>
/// <para>https://developer.apple.com/documentation/appstoreservernotifications/revocationtype</para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RevocationType
{
    /// <summary>
    /// The App Store refunded the transaction in full.
    /// <para>App Store 全额退款了交易。</para>
    /// </summary>
    REFUND_FULL,

    /// <summary>
    /// The App Store refunded the transaction in part.
    /// <para>App Store 部分退款了交易。</para>
    /// </summary>
    REFUND_PRORATED,

    /// <summary>
    /// The App Store revoked the transaction because the user revoked family sharing.
    /// <para>由于用户撤销了家庭共享，App Store 撤销了交易。</para>
    /// </summary>
    FAMILY_REVOKE,
}