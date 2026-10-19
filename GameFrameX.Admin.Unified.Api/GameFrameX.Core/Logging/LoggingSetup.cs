namespace GameFrameX.Core.Logging;

public static class LoggingSetup
{
    /// <summary>
    /// 日志注册。C171 起不再依赖静态 Logging.json：数据库/ES 日志 Writer 无条件注册，
    /// 实际启停由各 Writer 写路径读 <c>PlatformLoggingConfigProvider</c> 快照决定（保存后同 Host 即时生效）。
    /// C363 起补齐调度器：所有 Sink 由 <see cref="LoggingDispatchService"/> 后台消费，业务线程只入队。
    /// </summary>
    public static void AddLoggingSetup(this IServiceCollection services)
    {
        //// 控制台日志格式化
        //services.AddConsoleFormatter(options =>
        //{
        //    options.DateFormat = "yyyy-MM-dd HH:mm:ss(zzz) dddd";
        //    //options.WithTraceId = true; // 显示线程Id
        //    //options.WithStackFrame = true; // 显示程序集
        //});

        // 日志调度器（后台消费队列：文件 Sink + 数据库/ES Writer 的统一分发入口）
        services.AddSingleton<LoggingDispatchQueue>();
        services.AddSingleton<ILoggingDispatchQueue>(static serviceProvider => serviceProvider.GetRequiredService<LoggingDispatchQueue>());
        services.AddHostedService<LoggingDispatchService>();

        // 日志监听
        services.AddMonitorLogging(options =>
        {
            options.IgnorePropertyNames = new[] { "Byte", };
            options.IgnorePropertyTypes = new[] { typeof(byte[]), };
        });

        // 日志写入文件（文件日志为结构性参数，随 Host 重启生效）
        var loggingMonitorSettings = new LoggingMonitorSettings { JsonBehavior = JsonBehavior.OnlyJson, };
        Array.ForEach(new[] { LogLevel.Information, LogLevel.Warning, LogLevel.Error, }, logLevel =>
        {
            services.AddFileLogging(options =>
            {
                options.WithTraceId = true; // 显示线程Id
                options.WithStackFrame = true; // 显示程序集
                options.FileNameRule = fileName => string.Format(fileName, DateTime.Now, logLevel.ToString()); // 每天创建一个文件
                options.WriteFilter = logMsg => logMsg.LogLevel == logLevel; // 日志级别
                options.HandleWriteError = (writeError) => // 写入失败时启用备用文件
                {
                    var fallbackName = Path.GetFileNameWithoutExtension(writeError.CurrentFileName) + "-oops" + Path.GetExtension(writeError.CurrentFileName);
                    writeError.UseRollbackFileName(Path.Combine(Path.GetDirectoryName(writeError.CurrentFileName) ?? string.Empty, "data", "logs", fallbackName));
                };
                if (loggingMonitorSettings.JsonBehavior == JsonBehavior.OnlyJson)
                {
                    options.MessageFormat = LoggerFormatter.Json;
                    // options.MessageFormat = LoggerFormatter.JsonIndented;
                    options.MessageFormatRule = logMsg =>
                    {
                        var jsonString = logMsg.Context.Get("loggingMonitor");
                        return jsonString?.ToString();
                    };
                }
            });
        });

        // 日志写入数据库（Writer 写路径读快照 database.enabled 实现同 Host 动态关停）
        services.AddProjectDatabaseOperationLogging();

        // 日志写入ElasticSearch（Writer 写路径读快照 elasticSearch.enabled 实现同 Host 动态关停）
        services.AddProjectElasticSearchLogging();
    }
}
