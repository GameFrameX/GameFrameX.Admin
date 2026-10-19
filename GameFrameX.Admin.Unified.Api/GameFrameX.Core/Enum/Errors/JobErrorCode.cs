namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// Admin 任务调度错误码（段 311，见 <see cref="ErrorSegmentRegistry"/>；C265 自旧 D 码体系平移）。
/// </summary>
public enum JobErrorCode
{
    /// <summary>
    /// 脚本代码不能为空
    /// </summary>
    [ErrorText("脚本代码不能为空", "Script code cannot be empty")]
    ScriptCodeEmpty = 31100001,

    /// <summary>
    /// 脚本代码中的作业类，需要定义 [JobDetail] 特性
    /// </summary>
    [ErrorText("脚本代码中的作业类，需要定义 [JobDetail] 特性", "The job class in the script must define the [JobDetail] attribute")]
    JobDetailAttributeRequired = 31100002,

    /// <summary>
    /// 作业编号需要与脚本代码中的作业类 [JobDetail('jobId')] 一致
    /// </summary>
    [ErrorText("作业编号需要与脚本代码中的作业类 [JobDetail('jobId')] 一致", "The job ID must match [JobDetail('jobId')] in the script")]
    JobIdMismatchWithScript = 31100003,

    /// <summary>
    /// 禁止修改作业编号
    /// </summary>
    [ErrorText("禁止修改作业编号", "Changing the job ID is forbidden")]
    JobIdChangeForbidden = 31100004,

    /// <summary>
    /// 执行作业失败
    /// </summary>
    [ErrorText("执行作业失败", "Failed to execute the job")]
    JobExecutionFailed = 31100005,
}