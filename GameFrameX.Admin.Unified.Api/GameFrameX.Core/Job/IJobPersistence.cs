namespace GameFrameX.Core.Job;

public interface IJobPersistence
{
    Task<IEnumerable<SchedulerBuilder>> PreloadAsync(CancellationToken stoppingToken);
    Task<SchedulerBuilder> OnLoadingAsync(SchedulerBuilder builder, CancellationToken stoppingToken);
    Task OnChangedAsync(PersistenceContext context);
    Task OnTriggerChangedAsync(PersistenceTriggerContext context);
    Task OnExecutionRecordAsync(PersistenceExecutionRecordContext context);
}