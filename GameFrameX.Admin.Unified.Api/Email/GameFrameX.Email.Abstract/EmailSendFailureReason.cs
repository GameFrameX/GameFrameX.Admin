namespace GameFrameX.Email.Abstract;

/// <summary>
/// 邮件发送失败原因分类。
/// <para>Provider 实现内部校验 / 上游调用失败均映射到本枚举，便于上层决定日志提示与 fallback 策略。</para>
/// </summary>
public enum EmailSendFailureReason
{
    /// <summary>
    /// 缺凭据 / 缺必填字段。
    /// </summary>
    MissingCredentials = 1,

    /// <summary>
    /// 上游 Provider 调用失败（HTTP / SDK 异常）。
    /// </summary>
    UpstreamError = 2,

    /// <summary>
    /// 输入参数不合法。
    /// </summary>
    InvalidInput = 3,
}