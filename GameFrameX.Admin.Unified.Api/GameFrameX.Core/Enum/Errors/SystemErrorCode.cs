namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// 系统域错误码（段 100，见 <see cref="ErrorSegmentRegistry"/>）。
/// </summary>
public enum SystemErrorCode
{
    /// <summary>
    /// 内部错误
    /// </summary>
    [ErrorText("内部错误", "Internal error")] InnerError = 10000001,

    /// <summary>
    /// 平台系统邮件测试发送失败（详情见日志 reason）
    /// </summary>
    [ErrorText("平台系统邮件测试发送失败", "Platform system email test send failed")] SystemEmailTestSendFailed = 10000002,
}