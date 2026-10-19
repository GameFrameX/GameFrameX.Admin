using Microsoft.Extensions.Hosting;

namespace GameFrameX.Core.Job;

public sealed class ProjectSchedulerHostedService : BackgroundService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IEnumerable<IJobPersistence> _persistences;

    public ProjectSchedulerHostedService(ISchedulerFactory schedulerFactory, IEnumerable<IJobPersistence> persistences)
    {
        _schedulerFactory = schedulerFactory;
        _persistences = persistences;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var builder in ProjectTypeCatalog.EffectiveTypes.ScanToBuilders())
        {
            _schedulerFactory.AddJob(builder.GetJobBuilder());
        }

        foreach (var persistence in _persistences)
        {
            var builders = await persistence.PreloadAsync(stoppingToken);
            foreach (var builder in builders)
            {
                _schedulerFactory.AddJob(builder.GetJobBuilder());
            }
        }
    }
}