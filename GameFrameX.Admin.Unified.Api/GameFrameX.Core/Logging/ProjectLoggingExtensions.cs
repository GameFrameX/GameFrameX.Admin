using Microsoft.Extensions.Hosting;

namespace GameFrameX.Core.Logging;

/// <summary>
/// 项目日志注册入口（C363 接通真实实现，取代历史框架退役时留下的空桩）：
/// Monitor 采集中间件全局过滤器 + 文件日志 Sink + 数据库/ES 日志写入器，统一由
/// <see cref="LoggingDispatchService"/> 后台调度消费。
/// </summary>
public static class ProjectLoggingExtensions
{
    /// <summary>
    /// 注册 Monitor 日志采集中间件：向 MVC 管线挂全局 <see cref="MonitorLoggingFilter"/>（Order=-2000），
    /// 并缓存序列化忽略配置（IgnorePropertyNames / IgnorePropertyTypes）。
    /// </summary>
    public static IServiceCollection AddMonitorLogging(this IServiceCollection services, Action<ProjectLoggingOptions>? configure = null)
    {
        var options = new ProjectLoggingOptions();
        configure?.Invoke(options);
        services.AddSingleton(new MonitorLoggingEntry(options));
        services.AddSingleton<MonitorLoggingRuntimeState>();
        services.Configure<MvcOptions>(mvcOptions =>
        {
            mvcOptions.Filters.Add<MonitorLoggingFilter>(MonitorLoggingFilter.FilterOrder);
        });
        return services;
    }

    /// <summary>
    /// 注册文件日志 Sink：按级别过滤 + 文件名规则把日志以 JSON 行写入 data/logs（相对 Host ContentRoot）。
    /// 多次调用（如 Information/Warning/Error 各一档）注册多个 Sink 实例。
    /// </summary>
    public static IServiceCollection AddFileLogging(this IServiceCollection services, Action<ProjectLoggingOptions>? configure = null)
    {
        var options = new ProjectLoggingOptions();
        configure?.Invoke(options);
        services.AddSingleton(serviceProvider => new FileLoggingSink(options, serviceProvider.GetRequiredService<IHostEnvironment>().ContentRootPath));
        return services;
    }

    /// <summary>
    /// 注册数据库/ES 日志写入器：Writer 注册为单例，并与注册期配置（WriteFilter 分发语义）配对成
    /// <see cref="DatabaseLoggingSinkRegistration"/> 供调度器消费。
    /// </summary>
    public static IServiceCollection AddDatabaseLogging<TWriter>(this IServiceCollection services, Action<ProjectLoggingOptions>? configure = null)
        where TWriter : class, IDatabaseLoggingWriter
    {
        var options = new ProjectLoggingOptions();
        configure?.Invoke(options);
        services.AddSingleton<TWriter>();
        services.AddSingleton(serviceProvider => new DatabaseLoggingSinkRegistration(serviceProvider.GetRequiredService<TWriter>(), options));
        return services;
    }
}
