namespace GameFrameX.Core.Job;

/// <summary>
/// 作业执行接口。
/// </summary>
public interface IJob
{
    Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken);
}