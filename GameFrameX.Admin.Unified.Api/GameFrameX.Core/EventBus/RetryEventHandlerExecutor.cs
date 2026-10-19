namespace GameFrameX.Core.EventBus;

/// <summary>
/// 事件执行器-超时控制、失败重试熔断等等
/// </summary>
public class RetryEventHandlerExecutor : IEventHandlerExecutor
{
    public async Task ExecuteAsync(EventHandlerExecutingContext context, Func<EventHandlerExecutingContext, Task> handler, CancellationToken cancellationToken)
    {
        // 如果执行失败，每隔 1s 重试，最多三次
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                await handler(context);
                return;
            }
            catch when (attempt < 3)
            {
                await Task.Delay(1000, cancellationToken);
            }
        }
    }
}