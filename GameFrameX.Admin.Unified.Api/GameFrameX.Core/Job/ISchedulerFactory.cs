namespace GameFrameX.Core.Job;

public interface ISchedulerFactory
{
    void AddJob(JobBuilder builder);

    /// <summary>
    /// 注册作业并保留其触发器（触发循环按触发器评估执行）。
    /// </summary>
    /// <param name="builder">作业计划构建器（含触发器）。</param>
    void AddJob(SchedulerBuilder builder);

    ProjectScheduler? GetJob(string jobId);
    void RemoveJob(string jobId);
    void PauseAll();
    void StartAll();
    Task<ScheduleResult> TryRunJobAsync(string jobId);
    void CancelSleep();
    void PersistAll();
}