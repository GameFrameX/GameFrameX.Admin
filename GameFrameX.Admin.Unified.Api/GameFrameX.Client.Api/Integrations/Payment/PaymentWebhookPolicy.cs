using GameFrameX.Payment.Google.Option;

namespace GameFrameX.Client.Api.Integrations.Payment;

/// <summary>
/// 支付 Webhook 的纯策略判断，避免控制器中散落 provider 特定字符串。
/// </summary>
public static class PaymentWebhookPolicy
{
    private static readonly HashSet<string> XsollaSubscriptionNotifications = new(StringComparer.OrdinalIgnoreCase)
    {
        "payment",
        "cancel_subscription",
        "create_subscription",
        "non_renewal_subscription",
        "update_subscription",
    };

    /// <summary>
    /// Google Pub/Sub authenticated push 必须同时配置 audience 与签发 JWT 的服务账号邮箱。
    /// 缺任一项时 Webhook 入口 fail closed，避免退回“只校验 body”的不安全路径。
    /// </summary>
    public static bool HasGooglePubSubPushAuthenticationConfig(GooglePaymentOptionSetting setting)
    {
        return setting != null &&
               !string.IsNullOrWhiteSpace(setting.GooglePaymentPubSubPushAudience) &&
               !string.IsNullOrWhiteSpace(setting.GooglePaymentPubSubPushServiceAccountEmail);
    }

    /// <summary>
    /// 当前项目尚无 Xsolla 订阅权益模型；这些通知只能显式记录并 no-op，不能触发普通商品发货。
    /// </summary>
    public static bool IsXsollaSubscriptionNotification(string notificationType)
    {
        return !string.IsNullOrWhiteSpace(notificationType) &&
               XsollaSubscriptionNotifications.Contains(notificationType);
    }
}