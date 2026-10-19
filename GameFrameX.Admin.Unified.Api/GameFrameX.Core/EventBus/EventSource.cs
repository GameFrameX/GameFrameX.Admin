namespace GameFrameX.Core.EventBus;

/// <summary>
/// 事件源。
/// </summary>
public sealed class EventSource
{
    public EventSource(string eventId, object? payload)
    {
        EventId = eventId;
        Payload = payload;
    }

    public string EventId { get; }

    public object? Payload { get; }
}