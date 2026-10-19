namespace GameFrameX.Core.Job;

public interface ISchedulerFactory
{
    void AddJob(JobBuilder builder);
    ProjectScheduler? GetJob(string jobId);
    void RemoveJob(string jobId);
    void PauseAll();
    void StartAll();
    Task<ScheduleResult> TryRunJobAsync(string jobId);
    void CancelSleep();
    void PersistAll();
}