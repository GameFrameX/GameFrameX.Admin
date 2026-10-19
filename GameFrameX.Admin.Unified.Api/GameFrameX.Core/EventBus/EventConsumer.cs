#if false
namespace GameFrameX.Core.EventBus;

/// <summary>
/// Redis 消息扩展（已禁用，依赖 FullRedis）
/// </summary>
/// <typeparam name="T"></typeparam>
[Obsolete("Redis event bus is disabled")]
public class EventConsumer<T> : IDisposable
{
    private Task _consumerTask;
    private CancellationTokenSource _consumerCts;

    public IProducerConsumer<T> Consumer { get; }
    public FullRedis Builder { get; set; }
    public event EventHandler<T> Received;

    public EventConsumer(FullRedis redis, string routeKey)
    {
        Builder = redis;
        Consumer = Builder.GetQueue<T>(routeKey);
    }

    public void Start()
    {
        if (Consumer is null) throw new InvalidOperationException("Subscribe first using the Consumer.Subscribe() function");
        if (_consumerTask != null) return;

        _consumerCts = new CancellationTokenSource();
        var ct = _consumerCts.Token;
        _consumerTask = Task.Factory.StartNew(() =>
        {
            while (!ct.IsCancellationRequested)
            {
                var cr = Consumer.TakeOne(10);
                if (cr == null) continue;
                Received?.Invoke(this, cr);
            }
        }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    public async Task Stop()
    {
        if (_consumerCts == null || _consumerTask == null) return;
        _consumerCts.Cancel();
        try { await _consumerTask; }
        finally { _consumerTask = null; _consumerCts = null; }
    }

    public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_consumerTask != null) Stop().Wait();
            Builder.Dispose();
        }
    }
}

#endif