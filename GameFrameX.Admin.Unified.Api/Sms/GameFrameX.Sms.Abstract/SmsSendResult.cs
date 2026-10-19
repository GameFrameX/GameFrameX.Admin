namespace GameFrameX.Sms.Abstract;

/// <summary>
/// 短信发送结果。
/// </summary>
public sealed class SmsSendResult
{
    /// <summary>
    /// 发送是否成功。
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 失败原因分类（成功时为 null）。
    /// </summary>
    public SmsSendFailureReason? FailureReason { get; set; }

    /// <summary>
    /// 失败原始异常（成功时为 null；可选用于日志诊断，禁止直接对外）。
    /// </summary>
    public Exception? FailureException { get; set; }

    /// <summary>
    /// 上游 Provider 原始错误码（成功时为 null；阿里云 <c>Code</c> 等；可选用于日志诊断）。
    /// </summary>
    public string? UpstreamErrorCode { get; set; }

    /// <summary>
    /// 上游 Provider 原始错误描述（成功时为 null；阿里云 <c>Message</c> 等；可选用于日志诊断）。
    /// </summary>
    public string? UpstreamErrorMessage { get; set; }

    public static SmsSendResult Ok()
    {
        return new SmsSendResult { IsSuccess = true, };
    }

    public static SmsSendResult Fail(SmsSendFailureReason reason, Exception? exception = null, string? upstreamErrorCode = null, string? upstreamErrorMessage = null)
    {
        return new SmsSendResult
        {
            IsSuccess = false,
            FailureReason = reason,
            FailureException = exception,
            UpstreamErrorCode = upstreamErrorCode,
            UpstreamErrorMessage = upstreamErrorMessage,
        };
    }
}