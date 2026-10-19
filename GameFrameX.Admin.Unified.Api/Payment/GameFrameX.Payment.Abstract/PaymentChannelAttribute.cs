namespace GameFrameX.Payment.Abstract;

/// <summary>
/// 标注一个支付渠道的 Option 配置类，声明其渠道标识与展示名称。
/// </summary>
/// <remarks>
/// <para><see cref="Name"/> 必须与该渠道 <see cref="IPaymentChannelHandler.ChannelName"/> 严格对齐（按大小写不敏感比较），
/// 渠道名是贯穿 Schema 查询、配置反序列化、订单回调的稳定路由键，任一处不一致都会导致配置无法被正确读取。</para>
/// <para>由 <see cref="PaymentChannelRegistry"/> 在首次访问时反射收集，作为渠道绑定的唯一事实来源，
/// 消除此前在 SchemaProvider 字典、反序列化 switch 等多处各写一遍的散落映射。</para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class PaymentChannelAttribute : Attribute
{
    /// <summary>
    /// 渠道唯一标识（如 apple/google/xsolla/alipay），与对应 <see cref="IPaymentChannelHandler.ChannelName"/> 对齐。
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 渠道展示名称，供前端下拉、列表使用。
    /// </summary>
    public string Label { get; }

    public PaymentChannelAttribute(string name, string label)
    {
        Name = name;
        Label = label;
    }
}