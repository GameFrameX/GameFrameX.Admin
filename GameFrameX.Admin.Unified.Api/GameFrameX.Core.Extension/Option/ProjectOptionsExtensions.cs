using Microsoft.Extensions.DependencyInjection;

namespace GameFrameX.Core.Extension.Option;

/// <summary>
/// 项目 Options 注册扩展。
/// </summary>
public static class ProjectOptionsExtensions
{
    /// <summary>
    /// 将配置类型注册到 ASP.NET Core Options 容器。
    /// </summary>
    public static IServiceCollection AddConfigurableOptions<TOptions>(this IServiceCollection services)
        where TOptions : class
    {
        var sectionName = typeof(TOptions).GetCustomAttributes(typeof(OptionsSettingsAttribute), false)
                                          .OfType<OptionsSettingsAttribute>()
                                          .FirstOrDefault()?.Path
                          ?? typeof(TOptions).Name;
        services.AddOptions<TOptions>().BindConfiguration(sectionName);
        return services;
    }
}