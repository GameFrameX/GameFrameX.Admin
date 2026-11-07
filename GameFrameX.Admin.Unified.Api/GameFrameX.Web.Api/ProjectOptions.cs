using GameFrameX.Core.Extension.Option;
using GameFrameX.Core.Option;
using GameFrameX.Payment.Wechat.Option;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GameFrameX.Web.Api;

public static class ProjectOptions
{
    /// <summary>
    /// 注册项目配置选项
    /// </summary>
    public static IServiceCollection AddProjectOptions(this IServiceCollection services)
    {
        services.AddConfigurableOptions<SnowFlakeIdOptions>();
        services.AddConfigurableOptions<CacheOptions>();
        services.AddConfigurableOptions<WechatPayOptions>();


        return services;
    }
}