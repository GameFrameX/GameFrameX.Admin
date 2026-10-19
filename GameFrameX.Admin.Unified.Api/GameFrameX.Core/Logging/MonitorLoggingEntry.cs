namespace GameFrameX.Core.Logging;

/// <summary>
/// Monitor 采集中间件注册项：承载 <see cref="ProjectLoggingExtensions.AddMonitorLogging"/> 注册期配置
/// （序列化忽略的属性名/类型等），供 <see cref="MonitorLoggingFilter"/> 在参数/返回值序列化时应用。
/// </summary>
internal sealed class MonitorLoggingEntry
{
    public MonitorLoggingEntry(ProjectLoggingOptions options)
    {
        Options = options;
    }

    /// <summary>
    /// 注册期配置（IgnorePropertyNames / IgnorePropertyTypes 等，语义对齐历史框架 LoggingMonitor）。
    /// </summary>
    public ProjectLoggingOptions Options { get; }
}
