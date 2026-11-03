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
        services.AddConfigurableOptions<DbConnectionOptions>();
        services.AddConfigurableOptions<SnowFlakeIdOptions>();
        services.AddConfigurableOptions<CacheOptions>();
        services.AddConfigurableOptions<WechatPayOptions>();

        // Options 与数据库初始化共用主库环境变量覆盖及配置校验。
        services.AddSingleton<IPostConfigureOptions<DbConnectionOptions>, DbConnectionOptionsPostConfigure>();

        return services;
    }
}