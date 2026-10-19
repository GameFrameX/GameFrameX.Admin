namespace GameFrameX.Core.EventBus;

public static class ProjectEventBusSetup
{
    public static IServiceCollection AddEventBus(this IServiceCollection services, Action<ProjectEventBusOptions>? configure = null)
    {
        var options = new ProjectEventBusOptions();
        configure?.Invoke(options);
        services.AddSingleton<IEventPublisher, ProjectEventPublisher>();
        return services;
    }
}