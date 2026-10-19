namespace GameFrameX.Core.Job;

/// <summary>
/// 作业执行上下文。
/// </summary>
public sealed class JobExecutingContext
{
    public JobDetail? JobDetail { get; set; }

    public object? Result { get; set; }
}