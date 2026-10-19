// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// A value that indicates the extent to which the customer consumed the in-app purchase.
/// <para>指示客户消费应用内购买程度的值。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/consumptionstatus</para>
/// </summary>
public enum ConsumptionStatus
{
    /// <summary>
    /// Undeclared
    /// <para>未声明</para>
    /// </summary>
    UNDECLARED = 0,

    /// <summary>
    /// Not consumed
    /// <para>未消费</para>
    /// </summary>
    NOT_CONSUMED = 1,

    /// <summary>
    /// Partially consumed
    /// <para>部分消费</para>
    /// </summary>
    PARTIALLY_CONSUMED = 2,

    /// <summary>
    /// Fully consumed
    /// <para>完全消费</para>
    /// </summary>
    FULLY_CONSUMED = 3,
}