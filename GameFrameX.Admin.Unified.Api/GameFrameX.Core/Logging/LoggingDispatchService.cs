using Microsoft.Extensions.Hosting;

namespace GameFrameX.Core.Logging;

/// <summary>
/// 日志调度后台服务（C363 重建，取代历史框架退役时丢失的调度器）：消费 <see cref="LoggingDispatchQueue"/>，
/// 把每批消息按各 Sink 注册期 <see cref="ProjectLoggingOptions.WriteFilter"/> 语义分发到文件 Sink 与
/// <see cref="IDatabaseLoggingWriter"/> 集合。任一 Sink 写入异常只记录并继续——数据库日志
/// （C156 分表路由、C171 动态开关在 Writer 写路径内生效）与文件日志互不拖累，更不阻断业务线程。
/// </summary>
internal sealed class LoggingDispatchService : BackgroundService
{
    private const int MaxBatchSize = 256;

    private readonly LoggingDispatchQueue _queue;
    private readonly IReadOnlyList<FileLoggingSink> _fileSinks;
    private readonly IReadOnlyList<DatabaseLoggingSinkRegistration> _databaseSinks;
    private readonly ILogger<LoggingDispatchService> _logger;

    public LoggingDispatchService(LoggingDispatchQueue queue,
        IEnumerable<FileLoggingSink> fileSinks,
        IEnumerable<DatabaseLoggingSinkRegistration> databaseSinks,
        ILogger<LoggingDispatchService> logger)
    {
        _queue = queue;
        _fileSinks = fileSinks.ToList();
        _databaseSinks = databaseSinks.ToList();
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batch = new List<LogMessage>(MaxBatchSize);
        while (!stoppingToken.IsCancellationRequested)
        {
            batch.Clear();
            try
            {
                batch.Add(await _queue.ReadAsync(stoppingToken));
                while (batch.Count < MaxBatchSize && _queue.TryPeek(out _))
                {
                    // 批量收拢：同请求周期的多条消息一次分发，减少每条的 Sink 往返
                    batch.Add(await _queue.ReadAsync(stoppingToken));
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await DispatchAsync(batch);
            for (var processedIndex = 0; processedIndex < batch.Count; processedIndex++)
            {
                // 逐条终结排空计数：WaitDrainedAsync 的调用方看到 0 即代表全部 Sink 已处理完
                _queue.NotifyMessageProcessed();
            }
        }
    }

    private async Task DispatchAsync(List<LogMessage> batch)
    {
        foreach (var fileSink in _fileSinks)
        {
            try
            {
                await fileSink.WriteAsync(FilterForSink(batch, fileSink.Options));
            }
            catch (Exception exception)
            {
                DispatchLogError(exception, "文件日志写入失败");
            }
        }

        foreach (var databaseSink in _databaseSinks)
        {
            try
            {
                await databaseSink.Writer.WriteAsync(FilterForSink(batch, databaseSink.Options), false);
            }
            catch (Exception exception)
            {
                // DB/ES 写入失败隔离：吞并记录，不影响其它 Sink 与业务（C156：日志写入失败不得影响业务线程）
                DispatchLogError(exception, "数据库日志写入失败");
            }
        }
    }

    private static List<LogMessage> FilterForSink(List<LogMessage> batch, ProjectLoggingOptions options)
    {
        List<LogMessage>? filtered = null;
        foreach (var message in batch)
        {
            var accepted = options.WriteFilter == null || options.WriteFilter(message);
            if (accepted)
            {
                filtered ??= new List<LogMessage>(batch.Count);
                filtered.Add(message);
            }
        }

        return filtered ?? new List<LogMessage>(0);
    }

    private void DispatchLogError(Exception exception, string description)
    {
        // 日志系统自身的问题走 Console+Error 双通道留痕（不能再经本队列转发，防自激）
        _logger.LogError(exception, "{Description}", description);
        Console.Error.WriteLine($"{description}: {exception.Message}");
    }
}
