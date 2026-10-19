using Microsoft.AspNetCore.Builder;

namespace GameFrameX.Core.Job;

public static class ProjectScheduleSetup
{
    public static IServiceCollection AddSchedule(this IServiceCollection services, Action<ProjectScheduleOptions>? configure = null)
    {
        var options = new ProjectScheduleOptions();
        configure?.Invoke(options);
        options.Register(services);
        services.AddSingleton<ISchedulerFactory, ProjectSchedulerFactory>();
        services.AddHostedService<ProjectSchedulerHostedService>();
        return services;
    }

    public static IApplicationBuilder UseScheduleUI(this IApplicationBuilder app)
    {
        return app;
    }

    public static IEnumerable<SchedulerBuilder> ScanToBuilders(this IEnumerable<Type> types)
    {
        foreach (var jobType in types.Where(type => typeof(IJob).IsAssignableFrom(type) && type is { IsClass: true, IsAbstract: false, }))
        {
            var jobBuilder = JobBuilder.Create(jobType);
            var triggerBuilders = jobType.GetCustomAttributes(typeof(TriggerAttribute), true)
                                         .OfType<TriggerAttribute>()
                                         .Select(attribute => TriggerBuilder.Create(attribute.TriggerId ?? $"{jobBuilder.JobId}_trigger")
                                                                            .SetDescription(attribute.Description)
                                                                            .SetMaxNumberOfRuns(attribute.MaxNumberOfRuns)
                                                                            .SetRunOnStart(attribute.RunOnStart));
            yield return SchedulerBuilder.Create(jobBuilder, triggerBuilders.ToArray());
        }
    }

    public static SchedulerBuilder ScanToBuilder(this Type jobType)
    {
        return new[] { jobType, }.ScanToBuilders().Single();
    }
}