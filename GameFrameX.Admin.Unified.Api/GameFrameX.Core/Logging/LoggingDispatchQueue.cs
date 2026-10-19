using System.Threading.Channels;

namespace GameFrameX.Core.Logging;

/// <summary>
/// 日志调度队列默认实现：无界通道 + 待处理计数 + 排空信号。单消费者（<see cref="LoggingDispatchService"/>），
/// 入队永不阻塞业务线程；排空信号用锁保护计数与 TaskCompletionSource 的成对翻转，避免「计数已归零、
/// 信号未挂上」的丢唤醒竞态。ponytail: 排空判定按条计数逐条通知，消息量大时锁开销可接受；
/// 若未来日志量级显著增长，升级方向是按批次批次号水位（Watermark）通知。
/// </summary>
internal sealed class LoggingDispatchQueue : ILoggingDispatchQueue
{
    private readonly Channel<LogMessage> _channel = Channel.CreateUnbounded<LogMessage>(new UnboundedChannelOptions
    {
        SingleReader = true,
    });

    private readonly object _drainGate = new();
    private long _pendingCount;
    private TaskCompletionSource? _drainedSignal;

    public void Enqueue(LogMessage message)
    {
        lock (_drainGate)
        {
            _pendingCount++;
        }

        if (!_channel.Writer.TryWrite(message))
        {
            // 通道已关闭（宿主停机）：条目立即终结，防止排空等待永久挂起
            NotifyMessageProcessed();
        }
    }

    public Task WaitDrainedAsync(CancellationToken cancellationToken = default)
    {
        lock (_drainGate)
        {
            if (_pendingCount == 0)
            {
                return Task.CompletedTask;
            }

            _drainedSignal ??= new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            return _drainedSignal.Task.WaitAsync(cancellationToken);
        }
    }

    /// <summary>
    /// 读取下一条待调度消息（仅供后台调度服务调用）。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>消息读取任务。</returns>
    public ValueTask<LogMessage> ReadAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }

    /// <summary>
    /// 探测队列中是否还有待读消息（仅供后台调度服务批量收拢时调用，不出队）。
    /// </summary>
    internal bool TryPeek(out LogMessage message)
    {
        return _channel.Reader.TryPeek(out message!);
    }

    /// <summary>
    /// 通知单条消息已调度终结（写入成功或失败均算终结）。待处理计数归零时触发排空信号。
    /// </summary>
    internal void NotifyMessageProcessed()
    {
        TaskCompletionSource? signal = null;
        lock (_drainGate)
        {
            _pendingCount--;
            if (_pendingCount == 0)
            {
                signal = _drainedSignal;
                _drainedSignal = null;
            }
        }

        signal?.TrySetResult();
    }
}
