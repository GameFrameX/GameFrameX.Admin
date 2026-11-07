namespace GameFrameX.Core.Job;

public interface IJobPersistence
{
    Task<IEnumerable<SchedulerBuilder>> PreloadAsync(CancellationToken stoppingToken);
    Task<SchedulerBuilder> OnLoadingAsync(SchedulerBuilder builder, CancellationToken stoppingToken);
    Task OnChangedAsync(PersistenceContext context);
    Task OnTriggerChangedAsync(PersistenceTriggerContext context);
    Task OnExecutionRecordAsync(PersistenceExecutionRecordContext context);

    /// <summary>
    /// 注册期持久化：把扫描 / 预载合并后的作业与触发器 upsert 进数据库，
    /// 使内置作业在「任务调度」页可见、触发器运行态可写回。无持久化能力的实现返回即可。
    /// </summary>
    /// <param name="jobDetail">作业元数据。</param>
    /// <param name="triggers">触发器集合。</param>
    /// <param name="stoppingToken">取消令牌。</param>
    Task PersistRegisteredAsync(JobDetail jobDetail, IReadOnlyList<Trigger> triggers, CancellationToken stoppingToken);
}
