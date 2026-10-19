namespace GameFrameX.Core.Logging;

public sealed class ProjectLoggingOptions
{
    public bool WithTraceId { get; set; }
    public bool WithStackFrame { get; set; }
    public bool IgnoreReferenceLoop { get; set; }
    public string[] IgnorePropertyNames { get; set; } = Array.Empty<string>();
    public Type[] IgnorePropertyTypes { get; set; } = Array.Empty<Type>();
    public LoggerFormatter MessageFormat { get; set; }
    public Func<LogMessage, string?>? MessageFormatRule { get; set; }
    public Func<LogMessage, bool>? WriteFilter { get; set; }

    /// <summary>
    /// 文件名模板（仅文件日志 Sink 使用），占位符由 <see cref="FileNameRule"/> 填充；
    /// 默认值与旧 Logging.json 的 File.FileName 等价。
    /// </summary>
    public string FileName { get; set; } = "data/logs/{0:yyyyMMdd}_{1}.log";

    public Func<string, string>? FileNameRule { get; set; }
    public Action<ProjectLoggingWriteError>? HandleWriteError { get; set; }
}