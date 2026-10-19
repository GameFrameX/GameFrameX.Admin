using GameFrameX.Core.Extension.Json;
using GameFrameX.Core.Filter;

namespace GameFrameX.Core.Bootstrap;

/// <summary>
/// 三 host（Client / Hub / Game）共享的 MVC 注册扩展（C261，GFX-668）。
/// <para>收敛各 Program.cs 内联的 AddControllers 装配：共享请求/响应日志两件套（MvcFilterApp.Init，C236 收敛）
/// + PascalCase JSON 序列化（PropertyNamingPolicy 保持 null，前端契约依赖 PascalCase 字段名）。</para>
/// </summary>
public static class HostBootstrapMvcExtensions
{
    /// <summary>
    /// 注册共享 MVC 装配（MvcFilterApp.Init + PascalCase JSON），并允许 host 追加自有 MvcOptions 配置
    /// （如 Client.Api 的 PenaltyLoginBlockedExceptionFilter）。
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="configureControllers">host 特有的 MvcOptions 追加配置，可为 null。</param>
    public static IMvcBuilder AddHostControllers(this IServiceCollection services, Action<MvcOptions>? configureControllers = null)
    {
        return services.AddControllers(option =>
                       {
                           MvcFilterApp.Init(option.Filters);
                           // 客户端（WebGL 小游戏与存量老包）对 string 字段会发 null/空串
                           // （如 DeviceModel/PackageName/CurrentTask），NRT 隐式 required 会直接
                           // 400 拦截（2026-09-10 线上实测：GameUserArchive/Upload 与
                           // GameRemoteConfig/GetInfo 全断，含所有已发版本）。关闭隐式校验，
                           // null 由 DTO 字段初值与 action 防御兜底。
                           option.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                           configureControllers?.Invoke(option);
                       })
                       .AddJsonOptions(config =>
                       {
                           // 修改Json 序列化格式为不修改（保持 PascalCase 契约）
                           config.JsonSerializerOptions.PropertyNamingPolicy = null;
                           // Id 结尾的 long 序列化为字符串（雪花 Id 防 JS 超 2^53 丢精度），与 Admin 同一规则（C270）；
                           // 反向宽容读取（字符串/数字均可入参）
                           ProjectJsonSerializerOptions.ApplyLongIdentifierStringContract(config.JsonSerializerOptions);
                       });
    }
}