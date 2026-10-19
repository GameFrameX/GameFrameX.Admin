namespace GameFrameX.Payment.Abstract;

/// <summary>
/// 支付渠道清单项
/// </summary>
public class PaymentConfigChannel
{
    /// <summary>
    /// 渠道值
    /// </summary>
    public string Value { get; set; } = null!;

    /// <summary>
    /// 渠道显示名称
    /// </summary>
    public string Label { get; set; } = null!;
}