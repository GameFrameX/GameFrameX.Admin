namespace GameFrameX.Client.Api.Integrations.Payment.Dto.Google;

/// <summary>
/// Google Play 商店通知消息
/// </summary>
public class GoogleWebHookInputGooglePlayNotification
{
    /// <summary>
    /// 通知版本号
    /// </summary>
    public string Version { get; set; } = null!;

    /// <summary>
    /// 应用包名
    /// </summary>
    public string PackageName { get; set; } = null!;

    /// <summary>
    /// 事件发生时间戳(毫秒)
    /// </summary>
    public string EventTimeMillis { get; set; } = null!;

    /// <summary>
    /// 一次性商品购买通知信息
    /// </summary>
    public GoogleWebHookInputOneTimeProductNotification OneTimeProductNotification { get; set; } = null!;

    /// <summary>
    /// 作废购买的通知
    /// </summary>
    public GoogleWebHookInputVoidedPurchaseNotification VoidedPurchaseNotification { get; set; } = null!;

    /// <summary>Returns a string that represents the current object.</summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        return JsonHelper.Serialize(this);
    }
}