namespace GameFrameX.Core.EventBus;

/// <summary>
/// 反射调用项目内订阅者的轻量事件总线。
/// </summary>
public sealed class ProjectEventPublisher : IEventPublisher
{
    private readonly IServiceProvider _serviceProvider;

    public ProjectEventPublisher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task PublishAsync(string eventId, object? payload, CancellationToken cancellationToken = default)
    {
        var subscribers = _serviceProvider.GetServices<IEventSubscriber>();
        var context = new EventHandlerExecutingContext(new EventSource(eventId, payload));
        foreach (var subscriber in subscribers)
        {
            var handlers = subscriber.GetType()
                                     .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                                     .Where(method => method.GetCustomAttributes(typeof(EventSubscribeAttribute), true)
                                                            .OfType<EventSubscribeAttribute>()
                                                            .Any(attribute => attribute.EventId == eventId));
            foreach (var handler in handlers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var result = handler.Invoke(subscriber, new object[] { context, });
                if (result is Task task)
                {
                    await task;
                }
            }
        }
    }
}