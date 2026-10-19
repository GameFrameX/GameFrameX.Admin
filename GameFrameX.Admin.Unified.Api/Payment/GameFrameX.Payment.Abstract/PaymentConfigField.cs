namespace GameFrameX.Payment.Abstract;

/// <summary>
/// 支付配置字段描述
/// </summary>
public class PaymentConfigField
{
    /// <summary>
    /// 字段键，与 Option 类属性名严格一致
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// 显示名称
    /// </summary>
    public string Label { get; set; } = null!;

    /// <summary>
    /// 字段类型：text | secret | multiline | number | bool | select
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// 是否必填
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// 帮助文案
    /// </summary>
    public string Help { get; set; } = null!;

    /// <summary>
    /// 默认值
    /// </summary>
    public object Default { get; set; } = null!;

    /// <summary>
    /// 下拉选项（仅 select 类型）
    /// </summary>
    public List<PaymentConfigOption> Options { get; set; } = null!;
}