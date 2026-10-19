// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// A value that indicates whether the order ID in the request is valid for your app.
/// <para>指示请求中的订单 ID 对您的应用是否有效的值。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/orderlookupstatus</para>
/// </summary>
public enum OrderLookupStatus
{
    /// <summary>
    /// The order ID is valid.
    /// <para>订单 ID 有效。</para>
    /// </summary>
    VALID = 0,

    /// <summary>
    /// The order ID is invalid.
    /// <para>订单 ID 无效。</para>
    /// </summary>
    INVALID = 1,
}