// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The code of the reason for the subscription-renewal-date extension.
/// <para>延长订阅续期日期的原因代码。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/extendreasoncode</para>
/// </summary>
public enum ExtendReasonCode
{
    /// <summary>
    /// Undeclared; no information provided.
    /// <para>未声明；未提供信息。</para>
    /// </summary>
    UNDECLARED = 0,

    /// <summary>
    /// The customer canceled the subscription because of a service issue or outage.
    /// <para>客户因服务问题或中断而取消订阅。</para>
    /// </summary>
    CUSTOMER_SATISFACTION = 1,

    /// <summary>
    /// The customer canceled the subscription for another reason.
    /// <para>客户因其他原因取消订阅。</para>
    /// </summary>
    OTHER = 2,

    /// <summary>
    /// The customer canceled the subscription because of a service issue or outage.
    /// <para>客户因服务问题或中断而取消订阅。</para>
    /// </summary>
    SERVICE_ISSUE_OR_OUTAGE = 3,
}