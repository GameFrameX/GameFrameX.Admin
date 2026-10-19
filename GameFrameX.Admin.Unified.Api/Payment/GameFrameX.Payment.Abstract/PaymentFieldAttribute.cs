namespace GameFrameX.Payment.Abstract;

/// <summary>
/// 支付配置字段元数据标注，用于反射生成前端表单 schema
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class PaymentFieldAttribute : Attribute
{
    /// <summary>
    /// 字段显示名称
    /// </summary>
    public string Label { get; set; } = null!;

    /// <summary>
    /// 是否为敏感字段（密钥/私钥/证书）
    /// </summary>
    public bool Sensitive { get; set; }

    /// <summary>
    /// 是否为多行大文本（证书/私钥/JSON 内容）
    /// </summary>
    public bool Multiline { get; set; }

    /// <summary>
    /// 是否必填
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// 帮助文案
    /// </summary>
    public string Help { get; set; } = null!;

    /// <summary>
    /// 下拉选项原始值列表（如 "sandbox","production"）
    /// </summary>
    public string[] Options { get; set; } = null!;

    /// <summary>
    /// 是否跳过该属性（只读 computed 属性不参与表单）
    /// </summary>
    public bool Ignore { get; set; }
}