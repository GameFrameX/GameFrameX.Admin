using System.Collections.Concurrent;
using System.Threading.Channels;

namespace GameFrameX.Core.Service.TenantMenuSync;

/// <summary>
/// 租户菜单同步队列（C353 T6）。进程内 unbounded Channel + 待处理去重集：
/// 同一租户在队列中至多一个待处理项（新触发在执行中到达会被记录，执行完成后立即再次执行；输入在执行期现场采集，天然取到最新状态）。
/// <para>失败延迟重入队走 <see cref="ScheduleDelayed"/>（一次性 System.Threading.Timer 到点再入队），
/// 不占用消费者并发槽位（lesson：绝不在并发槽内 sleep 等待）。</para>
/// </summary>
public sealed class TenantMenuSyncQueue
{
    /// <summary>
    /// 队列项（TenantId + 触发原因，原因进操作日志）
    /// </summary>
    public sealed record QueueItem(long TenantId, string Reason);

    private readonly Channel<QueueItem> _channel = Channel.CreateUnbounded<QueueItem>(new UnboundedChannelOptions
    {
        SingleReader = false,
        SingleWriter = false,
    });

    private readonly ConcurrentDictionary<long, byte> _pending = new();

    /// <summary>
    /// 入队（去重：该租户已有待处理项时返回 false）。
    /// </summary>
    public bool TryEnqueue(long tenantId, string reason)
    {
        if (!_pending.TryAdd(tenantId, 0))
        {
            return false;
        }

        _channel.Writer.TryWrite(new QueueItem(tenantId, reason));
        return true;
    }

    /// <summary>
    /// 读取下一项（阻塞直到有项或队列完成 / 取消）。
    /// </summary>
    public async Task<QueueItem> ReadAsync(CancellationToken cancellationToken)
    {
        return await _channel.Reader.ReadAsync(cancellationToken);
    }

    /// <summary>
    /// 消费者取出项后立即解除去重占位（执行期间允许新触发重新入队）。
    /// </summary>
    public void MarkDequeued(long tenantId)
    {
        _pending.TryRemove(tenantId, out _);
    }

    /// <summary>
    /// 延迟重入队（失败退避专用）：一次性 Timer 到点再 TryEnqueue，期间不占用任何消费者槽位；
    /// 若到点前该租户已被其它触发重新入队则静默丢弃（幂等，无需重复执行）。
    /// </summary>
    public void ScheduleDelayed(long tenantId, string reason, TimeSpan delay)
    {
        if (delay < TimeSpan.Zero)
        {
            delay = TimeSpan.Zero;
        }

        // 一次性自释放 Timer：到点先 Dispose 再入队；等待期间由 Timer 队列根引用保活，
        // 泄漏上限 = 退避等待中的租户数（有界，且队列随进程终止一并回收）。
        Timer? timer = null;
        timer = new Timer(_ =>
        {
            timer?.Dispose();
            timer = null;
            TryEnqueue(tenantId, reason);
        }, null, delay, Timeout.InfiniteTimeSpan);
    }

    /// <summary>
    /// 停止接收新项（进程关闭）。
    /// </summary>
    public void Complete()
    {
        _channel.Writer.TryComplete();
    }
}
