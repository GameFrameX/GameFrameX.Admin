using System.Globalization;
using GameFrameX.Core.Localization.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;

namespace GameFrameX.Core.Bootstrap;

/// <summary>
/// 请求本地化共享装配：按请求 Accept-Language 设置 <see cref="CultureInfo.CurrentUICulture"/>，
/// 使 <see cref="ErrorLocalizer"/> 在异常处理时按请求语言自动返回枚举成员 [ErrorText] 中英双语文案；默认 zh-CN。
/// </summary>
public static class HostBootstrapRequestLocalizationExtensions
{
    /// <summary>
    /// 注册请求本地化中间件（支持 zh-CN / en-US，默认 zh-CN，不回写 Content-Language 响应头）。
    /// </summary>
    public static IApplicationBuilder UseHostRequestLocalization(this IApplicationBuilder app)
    {
        var options = new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(ErrorLocalizer.DefaultCulture),
            SupportedCultures = new List<CultureInfo> { new(ErrorLocalizer.DefaultCulture), new(ErrorLocalizer.EnglishCulture), },
            SupportedUICultures = new List<CultureInfo> { new(ErrorLocalizer.DefaultCulture), new(ErrorLocalizer.EnglishCulture), },
            ApplyCurrentCultureToResponseHeaders = false,
        };
        return app.UseRequestLocalization(options);
    }
}