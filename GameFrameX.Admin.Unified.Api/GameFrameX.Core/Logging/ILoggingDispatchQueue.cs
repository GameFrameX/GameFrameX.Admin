namespace GameFrameX.Core.Logging;

/// <summary>
/// 日志调度队列。Monitor 采集中间件把 <see cref="LogMessage"/> 投递进来，由后台调度服务
/// (<see cref="LoggingDispatchService"/>) 异步消费写入各日志 Sink——业务线程只入队不等待，
/// 写入失败不影响业务响应（C363 重建管线，取代历史框架退役时丢失的日志调度器）。
/// </summary>
public interface ILoggingDispatchQueue
{
    /// <summary>
    /// 投递一条日志消息。非阻塞：仅写入内存通道，任何写入器异常都不会传播到调用方。
    /// </summary>
    /// <param name="message">日志消息。</param>
    void Enqueue(LogMessage message);

    /// <summary>
    /// 等待已入队消息全部被调度处理完（含各 Sink 写入完成或失败终结）。集成测试用它做
    /// 「真实调用 → 行落库」的确定性同步；宿主停机时也可用于收尾排空。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>排空完成的任务。</returns>
    Task WaitDrainedAsync(CancellationToken cancellationToken = default);
}
