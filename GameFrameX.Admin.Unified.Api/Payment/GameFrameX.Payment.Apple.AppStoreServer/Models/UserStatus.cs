// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The status of a customer’s account within your app.
/// <para>您应用中客户帐户的状态。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/userstatus</para>
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// Undeclared status.
    /// <para>未声明状态。</para>
    /// </summary>
    UNDECLARED = 0,

    /// <summary>
    /// The account is active.
    /// <para>帐户处于活动状态。</para>
    /// </summary>
    ACTIVE = 1,

    /// <summary>
    /// The account is suspended.
    /// <para>帐户已暂停。</para>
    /// </summary>
    SUSPENDED = 2,

    /// <summary>
    /// The account is terminated.
    /// <para>帐户已终止。</para>
    /// </summary>
    TERMINATED = 3,

    /// <summary>
    /// The account has limited access.
    /// <para>帐户访问受限。</para>
    /// </summary>
    LIMITED_ACCESS = 4,
}