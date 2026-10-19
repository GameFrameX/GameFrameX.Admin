namespace GameFrameX.Core.EventBus;

/// <summary>
/// 标记订阅方法处理的事件 ID。
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class EventSubscribeAttribute : Attribute
{
    public EventSubscribeAttribute(string eventId)
    {
        EventId = eventId;
    }

    /// <summary>
    /// 事件 ID。
    /// </summary>
    public string EventId { get; }
}