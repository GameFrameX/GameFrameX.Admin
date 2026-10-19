namespace GameFrameX.Core.EventBus;

/// <summary>
/// 事件处理执行器。
/// </summary>
public interface IEventHandlerExecutor
{
    /// <summary>
    /// 执行事件处理函数。
    /// </summary>
    Task ExecuteAsync(EventHandlerExecutingContext context, Func<EventHandlerExecutingContext, Task> handler, CancellationToken cancellationToken);
}