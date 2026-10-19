namespace GameFrameX.Payment.Abstract;

/// <summary>
/// 字段下拉选项
/// </summary>
public class PaymentConfigOption
{
    /// <summary>
    /// 选项值
    /// </summary>
    public string Value { get; set; } = null!;

    /// <summary>
    /// 选项显示名称
    /// </summary>
    public string Label { get; set; } = null!;
}