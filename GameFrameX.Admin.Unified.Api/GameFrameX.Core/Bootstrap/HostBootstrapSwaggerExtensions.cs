using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;

namespace GameFrameX.Core.Bootstrap;

/// <summary>
/// 三 host（Client / Hub / Game）共享的 Swagger 启动装配扩展（C261，GFX-668）。
/// <para>收敛各 Program.cs 内联的 <c>AddSwaggerService</c> 私有方法与 enableSwagger 判断。
/// Admin host 的 Swagger 语义不同（单文档 + AdminResultOperationFilter 包装），不适用本扩展。</para>
/// </summary>
public static class HostBootstrapSwaggerExtensions
{
    /// <summary>
    /// 判定 host 是否暴露 Swagger：开发环境默认开启；生产/Docker 通过配置 <c>Swagger:Enabled</c>
    /// （环境变量 Swagger__Enabled）显式开启，兼顾「容器内可访问」与「生产默认不外泄文档」。
    /// </summary>
    /// <param name="environment">host 环境。</param>
    /// <param name="configuration">host 配置。</param>
    public static bool IsHostSwaggerEnabled(this IWebHostEnvironment environment, IConfiguration configuration)
    {
        return environment.IsDevelopment() || configuration.GetValue("Swagger:Enabled", false);
    }

    /// <summary>
    /// 注册 Swagger 生成器：文档标题取入口程序集名，递归收录 XML 注释，按相对路径排序 action。
    /// </summary>
    /// <param name="services">服务集合。</param>
    public static IServiceCollection AddHostSwaggerGen(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(swaggerGenOptions =>
        {
            var title = Assembly.GetEntryAssembly()?.GetName().Name ?? "GameFrameX";
            swaggerGenOptions.SwaggerDoc("v1", new OpenApiInfo { Title = title, Version = "v1", });

            var directoryInfo = new DirectoryInfo(AppContext.BaseDirectory);
            var fileList = directoryInfo.GetFiles("*.xml", SearchOption.AllDirectories);
            foreach (var fileInfo in fileList)
            {
                swaggerGenOptions.IncludeXmlComments(fileInfo.FullName, true);
            }

            swaggerGenOptions.OrderActionsBy(s => s.RelativePath);
        });
        return services;
    }

    /// <summary>
    /// 暴露 Swagger 文档与 UI；仅在 <see cref="IsHostSwaggerEnabled"/> 为真时调用。
    /// </summary>
    /// <param name="app">host 应用。</param>
    public static void UseHostSwaggerUi(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
}