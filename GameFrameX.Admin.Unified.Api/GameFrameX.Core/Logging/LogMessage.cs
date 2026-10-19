namespace GameFrameX.Core.Logging;

public sealed class LogMessage
{
    public LogLevel LogLevel { get; set; }
    public string? LogName { get; set; }
    public Exception? Exception { get; set; }
    public DateTime LogDateTime { get; set; } = DateTime.Now;
    public EventId EventId { get; set; }
    public int ThreadId { get; set; } = Environment.CurrentManagedThreadId;
    public string? TraceId { get; set; }
    public string? Message { get; set; }
    public IDictionary<string, object?> Context { get; } = new Dictionary<string, object?>(StringComparer.Ordinal);
}