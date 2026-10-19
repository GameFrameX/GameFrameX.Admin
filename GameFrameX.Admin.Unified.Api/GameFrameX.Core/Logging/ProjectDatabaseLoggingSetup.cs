namespace GameFrameX.Core.Logging;

/// <summary>
/// 项目数据库日志注册入口。
/// </summary>
internal static class ProjectDatabaseLoggingSetup
{
    /// <summary>
    /// 注册操作日志数据库写入器。过滤语义取旧 Logging.json 的 <c>Database.MinimumLevel = Information</c>（最小级别）：
    /// Information 及以上放行——Monitor 正常调用为 Information，失败调用按 Error 输出，
    /// 由 <see cref="DatabaseLoggingWriter.WriteSingleAsync"/> 落 exception 表并填 Exception 字段（C363 验收 2/3 的前提）。
    /// </summary>
    public static void AddProjectDatabaseOperationLogging(this IServiceCollection services)
    {
        services.AddDatabaseLogging<DatabaseLoggingWriter>(options =>
        {
            options.WithTraceId = true;
            options.WithStackFrame = true;
            options.IgnoreReferenceLoop = false;
            options.WriteFilter = logMsg => logMsg.LogLevel >= LogLevel.Information;
        });
    }

    /// <summary>
    /// 注册 ElasticSearch 日志写入器，只接收 LoggingMonitor 结构化日志。
    /// </summary>
    public static void AddProjectElasticSearchLogging(this IServiceCollection services)
    {
        services.AddDatabaseLogging<ElasticSearchLoggingWriter>(options =>
        {
            options.WithTraceId = true;
            options.WithStackFrame = true;
            options.IgnoreReferenceLoop = false;
            options.MessageFormat = LoggerFormatter.Json;
            options.WriteFilter = logMsg => logMsg.LogName == "System.Logging.LoggingMonitor";
        });
    }
}