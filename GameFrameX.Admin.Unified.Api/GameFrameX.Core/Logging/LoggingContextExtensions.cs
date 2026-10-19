namespace GameFrameX.Core.Logging;

internal static class LoggingContextExtensions
{
    public static object? Get(this IDictionary<string, object?> context, string key)
    {
        return context.TryGetValue(key, out var value) ? value : null;
    }
}