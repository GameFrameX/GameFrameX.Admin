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
        services.AddConfigurableOptions<TenantMenuSyncOptions>();

        // Options 管线实例补跑 DbConnectionOptions.PostConfigure（GFX-682）：
        // 环境变量覆盖（GFX_MAIN_DB_*）与默认租户库推导只存在于 SqlSugarSetup 手工实例；
        // 注入 IOptions<DbConnectionOptions> 的消费方（SystemTenantService 等）此前拿到
        // 未推导的空实例，ConnectionConfigs 为 null 导致 AddTenantAsync 抛
        // ArgumentNullException(source)。此处让 Options 实例走同一套覆盖/推导/校验管线。
        services.AddSingleton<IPostConfigureOptions<DbConnectionOptions>, DbConnectionOptionsPostConfigure>();

        return services;
    }
}