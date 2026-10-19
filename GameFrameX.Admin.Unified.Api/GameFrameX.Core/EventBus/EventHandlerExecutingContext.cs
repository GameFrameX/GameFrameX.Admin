namespace GameFrameX.Core.EventBus;

/// <summary>
/// 事件处理上下文。
/// </summary>
public sealed class EventHandlerExecutingContext
{
    public EventHandlerExecutingContext(EventSource source)
    {
        Source = source;
    }

    public EventSource Source { get; }
}