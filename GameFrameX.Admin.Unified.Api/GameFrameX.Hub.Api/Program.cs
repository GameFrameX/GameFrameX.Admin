using GameFrameX.Hub.Api.Middleware;
using GameFrameX.Core.Cache;
using GameFrameX.Core.Middleware;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Hub.Api.Services;
using GameFrameX.Core.Bootstrap;
using GameFrameX.Foundation.Options;
using Serilog;
using SqlSugar;

namespace GameFrameX.Hub.Api;

internal static class Program
{
    private static void Main(string[] args)
    {
        // GFX-873 / C381 信号③：进程级未处理异常兜底（后台线程致命异常 / 主线程崩溃打印阶段），须早于任何可能崩溃的构建代码
        CrashExitWatchdog.EnableDomainUnhandledExceptionFallback();

        var builder = WebApplication.CreateBuilder(args);
        // builder.WebHost.UseUrls("http://0.0.0.0:20808");
        // builder.WebHost.UseSentry(options => { options.Dsn = "https://a680507a1eba0d3e2c458bc00b86d583@sentry.zilancreative.com/4"; });

        var launcherOptions = OptionsBuilder.CreateWithDebug<LauncherOptions>(args);

        // 共享启动装配（C261，GFX-668）：日志 / 雪花 / CORS / JWT / MVC / HealthChecks / Swagger 收敛至 Core Bootstrap
        builder.UseHostBootstrapLogging(launcherOptions);
        // Grafana Cloud OpenTelemetry（OTLP 三信号：traces/metrics/logs，logs 自动入 Loki）；凭证齐全才启用
        builder.UseHostOpenTelemetry("GameFrameX.Hub.Api");
        builder.Services.AddHostCors();

        // DataProtection 密钥环持久化到 data/DataProtection-Keys（容器内挂载卷 /app/data，与 Web/Client 同口径）
        builder.UseHostDataProtection();
        // 紧凑短 ID 布局 + WorkerId 防撞（C360，GFX-803）：读 SnowFlake:WorkerId（环境变量 SnowFlake__WorkerId），
        // 缺省兜底 2（Web.Api 配置 1 / Hub 2 / Client 3，单机全默认互不冲突）；同库实例必须互斥配置，越界 0-7 启动失败
        builder.UseSnowFlake(2);
        // 注册 JWT 鉴权（复用 Admin 同一份 JWTSettings 配置节，使 Admin 登录签发的 Token 可直接访问 Hub.Api）
        builder.Services.AddHostJwtBearer(builder.Configuration);
        builder.Services.AddHostControllers();
        builder.Services.AddHostHealthChecks();

        builder.Services.AddSingleton<LauncherOptions>(launcherOptions);
        // 租户解析接线（C260 共享扩展）：IHttpContextAccessor + 读取统一键 TenantContextAccessor.TenantIdKey 的解析器一次注册（HubTenantMiddleware 写入）
        var httpContextAccessor = new HttpContextAccessor();
        var tenantIdResolver = builder.Services.AddSqlSugarTenantResolution(httpContextAccessor);
        builder.Services.AddHttpClient();

        // 主库连接统一走 DbConnection 节 + GFX_MAIN_DB_* 单一入口（C269）；LauncherOptions 裸参数仅作兼容兜底
        var mainDbConfig = HostMainDbConnectionResolver.Resolve(builder.Configuration, launcherOptions);
        var sqlSugar = SqlSugarClientConfig.AddSqlSugarClientService(mainDbConfig, tenantIdResolver, HubUserAuditHandler.Handle(httpContextAccessor));

        builder.Services.AddSingleton<ISqlSugarClient>(sqlSugar); // 主库（连 SystemTenantEntity 等平台表）
        builder.Services.AddSingleton<HubTenantDbProvider>(); // 当前请求 TenantId -> 租户业务库连接（实时查库）
        builder.Services.AddSingleton<ITenantDbProvider>(sp => sp.GetRequiredService<HubTenantDbProvider>()); // 共享层密钥环 Resolver / 统一仓储依赖接口
        builder.Services.AddScoped(typeof(Repository<>)); // 仓储（按租户路由数据库）
        // 基类服务面：FeatureSwitchDeliveryResolver 等共享实现以 SqlSugarRepository<T> 为构造参数（C345 冻结契约），
        // DI 不做基类向上匹配，须显式注册开放泛型基类 -> Repository<T> 实现（与上行为不同 descriptor，仓储无状态可各自实例化）
        builder.Services.AddScoped(typeof(SqlSugarRepository<>), typeof(Repository<>));
        // 活动/功能开关投放解析器（C345 共享实现，依赖 Scoped 租户仓储，FeatureSwitchSync 端点消费）
        builder.Services.AddScoped<GameFrameX.Application.Game.App.FeatureSwitch.FeatureSwitchDeliveryResolver>();
        // OpenAccess 机器凭证中间件 nonce 去重依赖链（与 Client.Api 同款注册）：ICache + SystemCacheService（仅依赖 ICache + IOptions<CacheOptions>）
        builder.Services.AddCache();
        builder.Services.AddSingleton<SystemCacheService>();
        builder.Services.AddSerilog();

        // Swagger 注册与暴露统一用配置开关（与 Web.Api 一致）：开发环境默认开，生产/Docker 用 Swagger:Enabled（环境变量 Swagger__Enabled）开启。
        var enableSwagger = builder.Environment.IsHostSwaggerEnabled(builder.Configuration);
        if (enableSwagger)
        {
            builder.Services.AddHostSwaggerGen();
        }

        var app = builder.Build();

        // GFX-873 / C381 信号②：host 已停（StopHost / 停机）后进程 15s 未退出即强退——
        // BackgroundService 启动失败时 Run 正常返回无异常，仅此信号覆盖该类僵死
        var applicationLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
        applicationLifetime.ApplicationStopped.Register(() => CrashExitWatchdog.Arm("GameFrameX.Hub.Api host 已停止"));

        app.UseMiddleware<OptimisticConcurrencyMiddleware>();

        if (enableSwagger)
        {
            app.UseHostSwaggerUi();
        }

        // 设置跨域
        app.UseHostCors();
        // 请求语言解析（Accept-Language → CurrentUICulture，错误码文案按请求语言返回）
        app.UseHostRequestLocalization();
        // 鉴权与授权（顺序：UseAuthentication 必须在 UseAuthorization 之前）
        app.UseAuthentication();
        app.UseAuthorization();
        // Hub 租户中间件：拒绝超级管理员、统一将租户 Id 写入 HttpContext.Items（鉴权请求取 JWT，匿名 WebHook 取请求头）
        app.UseWhen(context => !context.Request.Path.StartsWithSegments("/health"),
                    branch => branch.UseMiddleware<HubTenantMiddleware>());

        // 应用开放访问凭证鉴权（机器对机器调用，如打包上报）：声明租户由 HubTenantMiddleware 写入，本中间件校验凭证 + 签名 + scope
        app.UseWhen(context => context.Request.Path.StartsWithSegments("/api/PackageBuildReport", StringComparison.OrdinalIgnoreCase),
                    branch => branch.UseMiddleware<TenantGameAppOpenAccessAuthMiddleware>());

        // 功能开关服务器同步（C347）：游戏服务器机器凭证同步通道，同一 OpenAccess 中间件鉴权（scope = FeatureSwitchSync 路由前缀）
        app.UseWhen(context => context.Request.Path.StartsWithSegments("/api/" + GameFrameX.Hub.Api.Const.OpenAccessScopeConst.FeatureSwitchSync, StringComparison.OrdinalIgnoreCase),
                    branch => branch.UseMiddleware<TenantGameAppOpenAccessAuthMiddleware>());

        app.MapHostHealthChecks();
        app.MapControllers();

        // GFX-873 / C381 信号①：逃逸 Run 的未处理异常若在运行时传播路径僵死（linux/arm64 容器实测），
        // watchdog 超时强退保证进程必死，容器 restart 策略才可触发；原样重抛保留默认崩溃语义
        try
        {
            app.Run();
        }
        catch (Exception exception)
        {
            CrashExitWatchdog.Arm("GameFrameX.Hub.Api host 未处理异常", exception);
            throw;
        }
    }
}