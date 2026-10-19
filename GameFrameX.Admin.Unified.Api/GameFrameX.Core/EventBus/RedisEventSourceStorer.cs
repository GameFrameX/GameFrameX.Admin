#if false
using System.Threading.Channels;

namespace GameFrameX.Core.EventBus;

/// <summary>
/// Redis自定义事件源存储器（已禁用，依赖 FullRedis）
/// </summary>
[Obsolete("Redis event bus is disabled")]
public sealed class RedisEventSourceStorer : IEventSourceStorer, IDisposable
{
    private readonly EventConsumer<ChannelEventSource> _eventConsumer;
    private readonly Channel<IEventSource> _channel;
    private readonly FullRedis _redis;
    private readonly string _routeKey;

    public RedisEventSourceStorer(ICache redis, string routeKey, int capacity)
    {
        var boundedChannelOptions = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
        };

        _channel = Channel.CreateBounded<IEventSource>(boundedChannelOptions);
        _redis = redis as FullRedis;
        _routeKey = routeKey;
        _eventConsumer = new EventConsumer<ChannelEventSource>(_redis, _routeKey);
        _eventConsumer.Received += (send, cr) => { _channel.Writer.WriteAsync(cr); };
        _eventConsumer.Start();
    }

    public async ValueTask WriteAsync(IEventSource eventSource, CancellationToken cancellationToken)
    {
        if (eventSource == default) throw new ArgumentNullException(nameof(eventSource));
        if (eventSource is ChannelEventSource source)
        {
            var queue = _redis.GetQueue<ChannelEventSource>(_routeKey);
            await Task.Factory.StartNew(() => { queue.Add(source); }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
        else
        {
            await _channel.Writer.WriteAsync(eventSource, cancellationToken);
        }
    }

    public async ValueTask<IEventSource> ReadAsync(CancellationToken cancellationToken)
    {
        var eventSource = await _channel.Reader.ReadAsync(cancellationToken);
        return eventSource;
    }

    public async void Dispose()
    {
        await _eventConsumer.Stop();
        GC.SuppressFinalize(this);
    }
}

#endif