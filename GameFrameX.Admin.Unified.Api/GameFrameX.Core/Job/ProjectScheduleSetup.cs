using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace GameFrameX.Core.Job;

public static class ProjectScheduleSetup
{
    public static IServiceCollection AddSchedule(this IServiceCollection services, Action<ProjectScheduleOptions>? configure = null)
    {
        var options = new ProjectScheduleOptions();
        configure?.Invoke(options);
        options.Register(services);
        // TryAdd 防双注册：AdminScheduleSetup 与 GameScheduleSetup 都会走到这里，
        // 重复注册会得到两个调度器工厂与两个触发循环（双倍触发）。
        services.TryAddSingleton<ISchedulerFactory, ProjectSchedulerFactory>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, ProjectSchedulerHostedService>());
        return services;
    }

    public static IApplicationBuilder UseScheduleUI(this IApplicationBuilder app)
    {
        return app;
    }
    public static IEnumerable<SchedulerBuilder> ScanToBuilders(this IEnumerable<Type> types)
    {
        foreach (var jobType in types.Where(type => typeof(IJob).IsAssignableFrom(type) && type is { IsClass: true, IsAbstract: false, }))
        {
            var jobBuilder = JobBuilder.Create(jobType);
            var triggerBuilders = jobType.GetCustomAttributes(typeof(TriggerAttribute), true)
                                         .OfType<TriggerAttribute>()
                                         .Select(attribute => ToTriggerBuilder(jobBuilder, attribute));
            yield return SchedulerBuilder.Create(jobBuilder, triggerBuilders.ToArray());
        }
    }

    /// <summary>
    /// 把触发器特性转成触发器构建器，并把特性携带的调度参数（周期秒数 / cron 表达式）绑定到 Args。
    /// </summary>
    private static TriggerBuilder ToTriggerBuilder(JobBuilder jobBuilder, TriggerAttribute attribute)
    {
        var triggerBuilder = TriggerBuilder.Create(attribute.TriggerId ?? $"{jobBuilder.JobId}_trigger")
                                           .SetDescription(attribute.Description)
                                           .SetMaxNumberOfRuns(attribute.MaxNumberOfRuns)
                                           .SetRunOnStart(attribute.RunOnStart);
        switch (attribute)
        {
            case PeriodSecondsAttribute periodAttribute:
            {
                triggerBuilder.SetTriggerType("GameFrameX.Core", TriggerSchedule.PeriodTriggerType)
                              .SetArgs(TriggerSchedule.PeriodArguments(periodAttribute.Seconds));
                break;
            }

            case CronAttribute cronAttribute:
            {
                triggerBuilder.SetTriggerType("GameFrameX.Core", TriggerSchedule.CronTriggerType)
                              .SetArgs(TriggerSchedule.CronArguments(cronAttribute.Expression));
                break;
            }

            default:
            {
                // 无参 [Daily] 等特性不带调度参数：保持休眠（激活需产品裁决；日志硬删与审计留存 R3 相关）。
                break;
            }
        }

        return triggerBuilder;
    }

    public static SchedulerBuilder ScanToBuilder(this Type jobType)
    {
        return new[] { jobType, }.ScanToBuilders().Single();
    }
}