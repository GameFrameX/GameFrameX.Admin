namespace GameFrameX.Core.Logging;

public interface IDatabaseLoggingWriter
{
    Task WriteAsync(IReadOnlyList<LogMessage> logMsgs, bool flush);
}