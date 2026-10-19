namespace GameFrameX.Payment.Abstract;

/// <summary>
/// 支付配置 schema
/// </summary>
public class PaymentConfigSchema
{
    /// <summary>
    /// 渠道标识
    /// </summary>
    public string Channel { get; set; } = null!;

    /// <summary>
    /// 渠道显示名称
    /// </summary>
    public string Label { get; set; } = null!;

    /// <summary>
    /// 字段列表
    /// </summary>
    public List<PaymentConfigField> Fields { get; set; } = null!;
}