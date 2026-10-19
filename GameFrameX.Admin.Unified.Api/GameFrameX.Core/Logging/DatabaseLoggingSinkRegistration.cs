namespace GameFrameX.Core.Logging;

/// <summary>
/// 数据库日志 Sink 注册项：把 <see cref="IDatabaseLoggingWriter"/> 与其注册期配置
/// （<see cref="ProjectLoggingOptions.WriteFilter"/> 等）配对，供调度器按各自过滤语义分发。
/// 对应 <see cref="ProjectLoggingExtensions.AddDatabaseLogging{TWriter}"/> 的真实注册（C363 接通空桩）。
/// </summary>
internal sealed class DatabaseLoggingSinkRegistration
{
    public DatabaseLoggingSinkRegistration(IDatabaseLoggingWriter writer, ProjectLoggingOptions options)
    {
        Writer = writer;
        Options = options;
    }

    /// <summary>
    /// 数据库/ES 日志写入器（DI 单例，含 DatabaseLoggingWriter 与 ElasticSearchLoggingWriter）。
    /// </summary>
    public IDatabaseLoggingWriter Writer { get; }

    /// <summary>
    /// 注册期配置：WriteFilter 决定该写入器接收哪些日志（DB 收 ≥Information 最小级别；ES 仅收 LoggingMonitor 分类）。
    /// </summary>
    public ProjectLoggingOptions Options { get; }
}
