using GameFrameX.Payment.Abstract;

namespace GameFrameX.Payment.Apple.Models;

/// <summary>
/// Apple支付创建订单结果
/// </summary>
public class AppleCreateOrderResponse : BaseCreateOrderResponse
{
    /// <summary>
    /// 产品ID
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// 应用Bundle ID
    /// </summary>
    public string BundleId { get; set; } = string.Empty;
}