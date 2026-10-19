// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The payload data for a subscription-renewal-date extension notification.
/// <para>订阅续期日期延长通知的负载数据。</para>
/// <para>https://developer.apple.com/documentation/appstoreservernotifications/summary</para>
/// </summary>
public class Summary
{
    /// <summary>
    /// The server environment that the notification applies to, either sandbox or production.
    /// <para>通知适用的服务器环境，沙盒或生产环境。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/environment</para>
    /// </summary>
    public ApplePayEnvironment? Environment { get; set; }

    /// <summary>
    /// The unique identifier of the app that the notification applies to.
    /// <para>通知适用的应用的唯一标识符。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/appappleid</para>
    /// </summary>
    public long? AppAppleId { get; set; }

    /// <summary>
    /// The bundle identifier of the app.
    /// <para>应用的包标识符。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/bundleid</para>
    /// </summary>
    public string? BundleId { get; set; }

    /// <summary>
    /// The unique identifier for the product, that you create in App Store Connect.
    /// <para>您在 App Store Connect 中创建的产品的唯一标识符。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/productid</para>
    /// </summary>
    public string? ProductId { get; set; }

    /// <summary>
    /// The value that indicates the extent of the subscription renewal date extension.
    /// <para>指示订阅续期日期延长程度的值。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/requestidentifier</para>
    /// </summary>
    public string? RequestIdentifier { get; set; }

    /// <summary>
    /// A list of store countries or regions.
    /// <para>商店国家或地区列表。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/storefrontcountrycodelist</para>
    /// </summary>
    public List<string>? StorefrontCountryCodes { get; set; }

    /// <summary>
    /// The count of success.
    /// <para>成功计数。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/succeededcount</para>
    /// </summary>
    public long? SucceededCount { get; set; }

    /// <summary>
    /// The count of failure.
    /// <para>失败计数。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/failedcount</para>
    /// </summary>
    public long? FailedCount { get; set; }
}