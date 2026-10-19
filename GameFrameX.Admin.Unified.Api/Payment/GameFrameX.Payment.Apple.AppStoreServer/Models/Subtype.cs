// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text.Json.Serialization;

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// A string that provides details about the notification type.
/// <para>提供有关通知类型详细信息的字符串。</para>
/// <para>https://developer.apple.com/documentation/appstoreservernotifications/subtype</para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Subtype
{
    /// <summary>
    /// Applies to the SUBSCRIBED notificationType. A notification with this subtype indicates that the user purchased the subscription for the first time or that the user received access to the subscription through Family Sharing for the first time.
    /// <para>适用于 SUBSCRIBED 通知类型。具有此子类型的通知表示用户首次购买了订阅，或者用户首次通过家庭共享获得了订阅访问权限。</para>
    /// </summary>
    INITIAL_BUY,

    /// <summary>
    /// Applies to the SUBSCRIBED notificationType. A notification with this subtype indicates that the user resubscribed or received access to the same subscription or another subscription within the same subscription group.
    /// <para>适用于 SUBSCRIBED 通知类型。具有此子类型的通知表示用户重新订阅或获得了同一订阅组中同一订阅或另一订阅的访问权限。</para>
    /// </summary>
    RESUBSCRIBE,

    /// <summary>
    /// Applies to the DID_CHANGE_RENEWAL_PREF notificationType. A notification with this subtype indicates that the user downgraded their subscription. Downgrades take effect at the next renewal date.
    /// <para>适用于 DID_CHANGE_RENEWAL_PREF 通知类型。具有此子类型的通知表示用户降级了其订阅。降级将在下一个续期日期生效。</para>
    /// </summary>
    DOWNGRADE,

    /// <summary>
    /// Applies to the DID_CHANGE_RENEWAL_PREF notificationType. A notification with this subtype indicates that the user upgraded their subscription. Upgrades take effect immediately.
    /// <para>适用于 DID_CHANGE_RENEWAL_PREF 通知类型。具有此子类型的通知表示用户升级了其订阅。升级立即生效。</para>
    /// </summary>
    UPGRADE,

    /// <summary>
    /// Applies to the DID_CHANGE_RENEWAL_STATUS notificationType. A notification with this subtype indicates that the user enabled subscription auto-renewal.
    /// <para>适用于 DID_CHANGE_RENEWAL_STATUS 通知类型。具有此子类型的通知表示用户启用了订阅自动续期。</para>
    /// </summary>
    AUTO_RENEW_ENABLED,

    /// <summary>
    /// Applies to the DID_CHANGE_RENEWAL_STATUS notificationType. A notification with this subtype indicates that the user disabled subscription auto-renewal, or the App Store disabled subscription auto-renewal after the user requested a refund.
    /// <para>适用于 DID_CHANGE_RENEWAL_STATUS 通知类型。具有此子类型的通知表示用户禁用了订阅自动续期，或者在用户请求退款后 App Store 禁用了订阅自动续期。</para>
    /// </summary>
    AUTO_RENEW_DISABLED,

    /// <summary>
    /// Applies to the EXPIRED notificationType. A notification with this subtype indicates that the subscription expired after the user disabled subscription auto-renewal.
    /// <para>适用于 EXPIRED 通知类型。具有此子类型的通知表示用户禁用订阅自动续期后订阅已过期。</para>
    /// </summary>
    VOLUNTARY,

    /// <summary>
    /// Applies to the EXPIRED notificationType. A notification with this subtype indicates that the subscription expired because the billing retry period ended without a successful billing transaction.
    /// <para>适用于 EXPIRED 通知类型。具有此子类型的通知表示由于计费重试期结束而没有成功的计费交易，订阅已过期。</para>
    /// </summary>
    BILLING_RETRY,

    /// <summary>
    /// Applies to the PRICE_INCREASE notificationType. A notification with this subtype indicates that the system informed the user of the subscription price increase, but the user hasn't yet accepted it.
    /// <para>适用于 PRICE_INCREASE 通知类型。具有此子类型的通知表示系统已通知用户订阅价格上涨，但用户尚未接受。</para>
    /// </summary>
    PRICE_INCREASE,

    /// <summary>
    /// Applies to the DID_FAIL_TO_RENEW notificationType. A notification with this subtype indicates that the subscription failed to renew due to a billing issue.
    /// <para>适用于 DID_FAIL_TO_RENEW 通知类型。具有此子类型的通知表示由于计费问题，订阅续期失败。</para>
    /// </summary>
    GRACE_PERIOD,

    /// <summary>
    /// Applies to the EXPIRED notificationType. A notification with this subtype indicates that the subscription was not renewed because the app is in a pending state.
    /// <para>适用于 EXPIRED 通知类型。具有此子类型的通知表示由于应用处于待处理状态，订阅未续期。</para>
    /// </summary>
    PENDING,

    /// <summary>
    /// Applies to the PRICE_INCREASE notificationType. A notification with this subtype indicates that the user accepted the subscription price increase.
    /// <para>适用于 PRICE_INCREASE 通知类型。具有此子类型的通知表示用户接受了订阅价格上涨。</para>
    /// </summary>
    ACCEPTED,

    /// <summary>
    /// Applies to the EXPIRED notificationType. A notification with this subtype indicates that the subscription expired because the billing recovery period ended without a successful billing transaction.
    /// <para>适用于 EXPIRED 通知类型。具有此子类型的通知表示由于计费恢复期结束而没有成功的计费交易，订阅已过期。</para>
    /// </summary>
    BILLING_RECOVERY,

    /// <summary>
    /// Applies to the EXPIRED notificationType. A notification with this subtype indicates that the subscription expired because the product is no longer for sale.
    /// <para>适用于 EXPIRED 通知类型。具有此子类型的通知表示由于产品不再发售，订阅已过期。</para>
    /// </summary>
    PRODUCT_NOT_FOR_SALE,

    /// <summary>
    /// Applies to the RENEWAL_EXTENSION notificationType. A notification with this subtype indicates that the App Store server is checking to see if it should extend the subscription renewal date.
    /// <para>适用于 RENEWAL_EXTENSION 通知类型。具有此子类型的通知表示 App Store 服务器正在检查是否应延长订阅续期日期。</para>
    /// </summary>
    SUMMARY,

    /// <summary>
    /// Applies to the RENEWAL_EXTENSION notificationType. A notification with this subtype indicates that the subscription renewal date extension failed.
    /// <para>适用于 RENEWAL_EXTENSION 通知类型。具有此子类型的通知表示订阅续期日期延长失败。</para>
    /// </summary>
    FAILURE,

    /// <summary>
    /// Applies to the SUBSCRIBED notificationType. A notification with this subtype indicates that the user purchased the subscription, but the app didn't report the event.
    /// <para>适用于 SUBSCRIBED 通知类型。具有此子类型的通知表示用户购买了订阅，但应用未报告该事件。</para>
    /// </summary>
    UNREPORTED,
}