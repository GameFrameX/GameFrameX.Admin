namespace GameFrameX.Core.EventBus;

/// <summary>
/// 事件发布入口。
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// 发布项目事件并调用匹配订阅者。
    /// </summary>
    Task PublishAsync(string eventId, object? payload, CancellationToken cancellationToken = default);
}