using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using GameFrameX.Core.Api;
using Microsoft.Extensions.DependencyInjection;

namespace GameFrameX.Web.Api.ApiRouting;

public static class AdminApiMvcBuilderExtensions
{
    public static IMvcBuilder AddAdminServiceControllers(this IMvcBuilder builder)
    {
        builder.ConfigureApplicationPartManager(manager => { manager.FeatureProviders.Add(new AdminApiControllerFeatureProvider()); });
        builder.Services.AddSingleton<AdminApiRuntimeChangeProvider>();
        builder.Services.AddSingleton<IAdminApiRuntimeChangeProvider>(provider => provider.GetRequiredService<AdminApiRuntimeChangeProvider>());
        builder.Services.AddSingleton<IActionDescriptorChangeProvider>(provider => provider.GetRequiredService<AdminApiRuntimeChangeProvider>());
        // C391：模型绑定失败显式 400 防线（动态控制器无 [ApiController]，工厂原本失活）。
        // 仅拦绑定失败，校验特性违规维持历史放行语义（详见 AdminInvalidModelStateFilter 注释）。
        builder.Services.AddSingleton<AdminInvalidModelStateFilter>();
        builder.Services.Configure<MvcOptions>(options =>
        {
            options.Conventions.Add(new AdminApiControllerConvention());
            options.Filters.AddService<AdminInvalidModelStateFilter>();
        });
        return builder;
    }
}
