// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// A response that indicates the current status of a request to extend the subscription renewal date to all eligible subscribers.
/// <para>指示向所有符合条件的订阅者延长订阅续期日期的请求的当前状态的响应。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/massextendrenewaldatestatusresponse</para>
/// </summary>
public class MassExtendRenewalDateStatusResponse
{
    /// <summary>
    /// A string that contains a unique identifier you provide to track each subscription-renewal-date extension request.
    /// <para>包含您提供的用于跟踪每个订阅续期日期延长请求的唯一标识符的字符串。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/requestidentifier</para>
    /// </summary>
    public string? RequestIdentifier { get; set; }

    /// <summary>
    /// A Boolean value that indicates whether the App Store completed the request to extend a subscription renewal date to active subscribers.
    /// <para>指示 App Store 是否已完成向活跃订阅者延长订阅续期日期的请求的布尔值。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/complete</para>
    /// </summary>
    public bool? Complete { get; set; }

    /// <summary>
    /// The UNIX time, in milliseconds, that the App Store completes a request to extend a subscription renewal date for eligible subscribers.
    /// <para>App Store 完成向符合条件的订阅者延长订阅续期日期请求的 UNIX 时间（以毫秒为单位）。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/completedate</para>
    /// </summary>
    public long? CompleteDate { get; set; }

    /// <summary>
    /// The count of subscriptions that successfully receive a subscription-renewal-date extension.
    /// <para>成功接收订阅续期日期延长的订阅计数。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/succeededcount</para>
    /// </summary>
    public long? SucceededCount { get; set; }

    /// <summary>
    /// The count of subscriptions that fail to receive a subscription-renewal-date extension.
    /// <para>未能接收订阅续期日期延长的订阅计数。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/failedcount</para>
    /// </summary>
    public long? FailedCount { get; set; }
}