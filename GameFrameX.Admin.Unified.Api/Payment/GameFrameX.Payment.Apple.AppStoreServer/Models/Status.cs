// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The status of an auto-renewable subscription.
/// <para>自动续期订阅的状态。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/status</para>
/// </summary>
public enum Status
{
    /// <summary>
    /// The subscription is active.
    /// <para>订阅处于活动状态。</para>
    /// </summary>
    ACTIVE = 1,

    /// <summary>
    /// The subscription is expired.
    /// <para>订阅已过期。</para>
    /// </summary>
    EXPIRED = 2,

    /// <summary>
    /// The subscription is in a billing retry period.
    /// <para>订阅处于计费重试期。</para>
    /// </summary>
    BILLING_RETRY = 3,

    /// <summary>
    /// The subscription is in a billing grace period.
    /// <para>订阅处于计费宽限期。</para>
    /// </summary>
    BILLING_GRACE_PERIOD = 4,

    /// <summary>
    /// The subscription is revoked.
    /// <para>订阅已被撤销。</para>
    /// </summary>
    REVOKED = 5,
}