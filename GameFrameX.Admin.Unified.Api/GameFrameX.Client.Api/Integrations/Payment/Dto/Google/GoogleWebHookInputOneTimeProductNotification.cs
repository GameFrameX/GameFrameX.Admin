namespace GameFrameX.Client.Api.Integrations.Payment.Dto.Google;

/// <summary>
/// 一次性商品购买通知详情
/// </summary>
public class GoogleWebHookInputOneTimeProductNotification
{
    /// <summary>
    /// 通知版本号
    /// </summary>
    public string Version { get; set; } = null!;

    /// <summary>
    /// 通知类型
    /// 1: 购买完成
    /// 2: 取消
    /// 3: 退款
    /// </summary>
    public int NotificationType { get; set; }

    /// <summary>
    /// 购买令牌，用于验证购买
    /// </summary>
    public string PurchaseToken { get; set; } = null!;

    /// <summary>
    /// 商品SKU标识
    /// </summary>
    public string Sku { get; set; } = null!;
}