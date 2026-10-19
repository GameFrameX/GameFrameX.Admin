namespace GameFrameX.Sms.Abstract;

/// <summary>
/// 标注短信 Provider 实现类的渠道标识与展示名称。
/// <para>由 <see cref="SmsSenderRegistry"/> 在首次访问时反射收集，作为渠道绑定的唯一事实来源，
/// 消除此前散落在 <c>TenantSmsConfigEntityService.SendTest</c> 内联 SDK 调用中的重复分派逻辑。</para>
/// </summary>
/// <remarks>
/// <para><see cref="Name"/> 是短信渠道的稳定路由键（如 <c>Aliyun</c>），与 <see cref="TenantSmsConfigEntity.Provider"/> 字段值保持严格一致（大小写不敏感）。</para>
/// <para><see cref="Label"/> 供前端渠道下拉、诊断输出使用；保持人可读，不参与路由。</para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SmsProviderAttribute : Attribute
{
    /// <summary>
    /// 渠道唯一标识（如 <c>Aliyun</c>）。
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 渠道展示名称。
    /// </summary>
    public string Label { get; }

    public SmsProviderAttribute(string name, string label)
    {
        Name = name;
        Label = label;
    }
}