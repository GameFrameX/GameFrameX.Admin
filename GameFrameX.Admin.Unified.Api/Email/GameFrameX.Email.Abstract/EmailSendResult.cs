namespace GameFrameX.Email.Abstract;

/// <summary>
/// 邮件发送结果。
/// </summary>
public sealed class EmailSendResult
{
    /// <summary>
    /// 发送是否成功。
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 失败原因分类（成功时为 null）。
    /// </summary>
    public EmailSendFailureReason? FailureReason { get; set; }

    /// <summary>
    /// 失败原始异常（成功时为 null；可选用于日志诊断，禁止直接对外）。
    /// </summary>
    public Exception? FailureException { get; set; }

    public static EmailSendResult Ok()
    {
        return new EmailSendResult { IsSuccess = true, };
    }

    public static EmailSendResult Fail(EmailSendFailureReason reason, Exception? exception = null)
    {
        return new EmailSendResult { IsSuccess = false, FailureReason = reason, FailureException = exception, };
    }
}