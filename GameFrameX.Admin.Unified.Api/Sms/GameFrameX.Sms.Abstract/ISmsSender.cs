namespace GameFrameX.Sms.Abstract;

/// <summary>
/// 短信发送器抽象。
/// <para>各服务商实现类（Aliyun / 后续 TencentCloud / Huawei 等）实现本接口；通过 <see cref="SmsSenderRegistry"/> 按渠道名反射收集与发现。</para>
/// <para>实现类必须标注 <see cref="SmsProviderAttribute"/> 声明稳定 <see cref="SmsProviderAttribute.Name"/>，由 Registry 反射绑定。</para>
/// </summary>
public interface ISmsSender
{
    /// <summary>
    /// 渠道唯一标识（如 <c>Aliyun</c>），与 <see cref="SmsProviderAttribute.Name"/> 一致。
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// 发送短信。
    /// </summary>
    /// <param name="options">发送参数；渠道实现按需读取自身字段。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>发送结果；缺凭据 / 输入不合法 / 上游错误通过 <see cref="SmsSendResult.FailureReason"/> 区分。</returns>
    Task<SmsSendResult> SendAsync(SmsSendOptions options, CancellationToken cancellationToken = default);
}