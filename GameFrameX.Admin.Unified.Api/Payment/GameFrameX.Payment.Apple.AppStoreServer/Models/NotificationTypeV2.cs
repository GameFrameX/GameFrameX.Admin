// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text.Json.Serialization;

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The type that describes the in-app purchase or external purchase event for which the App Store sends the version 2 notification.
/// <para>描述 App Store 发送版本 2 通知的应用内购买或外部购买事件的类型。</para>
/// <para>https://developer.apple.com/documentation/appstoreservernotifications/notificationtype</para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationTypeV2
{
    /// <summary>
    /// A notification type that indicates the customer subscribed to an auto-renewable subscription.
    /// <para>指示客户订阅了自动续期订阅的通知类型。</para>
    /// </summary>
    SUBSCRIBED,

    /// <summary>
    /// A notification type that indicates the customer made a change in their subscription plan that takes effect at the next renewal.
    /// <para>指示客户更改了其订阅计划，并在下一次续期时生效的通知类型。</para>
    /// </summary>
    DID_CHANGE_RENEWAL_PREF,

    /// <summary>
    /// A notification type that indicates a change in the subscription renewal status.
    /// <para>指示订阅续期状态发生更改的通知类型。</para>
    /// </summary>
    DID_CHANGE_RENEWAL_STATUS,

    /// <summary>
    /// A notification type that indicates the customer redeemed a promotional offer or offer code.
    /// <para>指示客户兑换了促销优惠或优惠代码的通知类型。</para>
    /// </summary>
    OFFER_REDEEMED,

    /// <summary>
    /// A notification type that indicates a subscription renewal occurred.
    /// <para>指示发生了订阅续期的通知类型。</para>
    /// </summary>
    DID_RENEW,

    /// <summary>
    /// A notification type that indicates a subscription expired.
    /// <para>指示订阅已过期的通知类型。</para>
    /// </summary>
    EXPIRED,

    /// <summary>
    /// A notification type that indicates a subscription failed to renew due to a billing issue.
    /// <para>指示由于计费问题导致订阅续期失败的通知类型。</para>
    /// </summary>
    DID_FAIL_TO_RENEW,

    /// <summary>
    /// A notification type that indicates the billing grace period has expired without a successful renewal.
    /// <para>指示计费宽限期已过且未成功续期的通知类型。</para>
    /// </summary>
    GRACE_PERIOD_EXPIRED,

    /// <summary>
    /// A notification type that indicates the system has informed the customer of an auto-renewable subscription price increase.
    /// <para>指示系统已通知客户自动续期订阅价格上涨的通知类型。</para>
    /// </summary>
    PRICE_INCREASE,

    /// <summary>
    /// A notification type that indicates the App Store successfully refunded a transaction.
    /// <para>指示 App Store 成功退款交易的通知类型。</para>
    /// </summary>
    REFUND,

    /// <summary>
    /// A notification type that indicates the App Store declined a refund request initiated by the app developer.
    /// <para>指示 App Store 拒绝了应用开发者发起的退款请求的通知类型。</para>
    /// </summary>
    REFUND_DECLINED,

    /// <summary>
    /// A notification type that indicates the customer initiated a refund request for a consumable in-app purchase, and the App Store is requesting that you provide consumption data.
    /// <para>指示客户对消耗型应用内购买发起了退款请求，并且 App Store 请求您提供消耗数据的通知类型。</para>
    /// </summary>
    CONSUMPTION_REQUEST,

    /// <summary>
    /// A notification type that indicates the App Store extended the subscription renewal date for a specific subscription.
    /// <para>指示 App Store 延长了特定订阅的订阅续期日期的通知类型。</para>
    /// </summary>
    RENEWAL_EXTENDED,

    /// <summary>
    /// A notification type that indicates the App Store has revoked a family shared subscription from a user.
    /// <para>指示 App Store 已从用户撤销家庭共享订阅的通知类型。</para>
    /// </summary>
    REVOKE,

    /// <summary>
    /// A notification type that the App Store server sends when you request it by calling the Request a Test Notification endpoint.
    /// <para>App Store 服务器在您通过调用“请求测试通知”端点请求时发送的通知类型。</para>
    /// </summary>
    TEST,

    /// <summary>
    /// A notification type that indicates the App Store is attempting to extend the subscription renewal date.
    /// <para>指示 App Store 正在尝试延长订阅续期日期的通知类型。</para>
    /// </summary>
    RENEWAL_EXTENSION,

    /// <summary>
    /// A notification type that indicates the App Store reversed a previously granted refund.
    /// <para>指示 App Store 撤销了先前授予的退款的通知类型。</para>
    /// </summary>
    REFUND_REVERSED,

    /// <summary>
    /// A notification type that indicates the App Store created an external purchase token.
    /// <para>指示 App Store 创建了外部购买令牌的通知类型。</para>
    /// </summary>
    EXTERNAL_PURCHASE_TOKEN,

    /// <summary>
    /// A notification type that indicates the App Store charged a customer for a one-time in-app purchase.
    /// <para>指示 App Store 向客户收取一次性应用内购买费用的通知类型。</para>
    /// </summary>
    ONE_TIME_CHARGE,

    /// <summary>
    /// A notification type that indicates the customer revoked their consent for authorized means of bill payment.
    /// <para>指示客户撤销了对授权账单支付方式的同意的通知类型。</para>
    /// </summary>
    RESCIND_CONSENT,
}