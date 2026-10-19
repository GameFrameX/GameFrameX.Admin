using GameFrameX.Client.Api.Services.Auth;
using GameFrameX.Client.Api.Services.GameLoginTicket;
using GameFrameX.Client.Api.Services.PackageVersion;
using GameFrameX.Client.Api.Services.RemoteConfig;
using GameFrameX.Client.Api.Services.PublicCallback;
using GameFrameX.Client.Api.Services.PayVerification;
using GameFrameX.Entity.Tenant.Game.RemoteConfig;
using GameFrameX.Entity.Tenant.Game.Release;
using GameFrameX.Application.Game.Penalty.GamePenalty;
using GameFrameX.Application.Game.User.GameUser;
using GameFrameX.Application.Game.Order.GameOrder;
using GameFrameX.Client.Api.Filter;
using GameFrameX.Client.Api.Middleware;
using GameFrameX.Client.Api.Services;
using GameFrameX.Client.Api.Services.ClientApiTenantOptions;
using GameFrameX.Client.Api.Services.Email;
using GameFrameX.Client.Api.Integrations.Payment;
using GameFrameX.Client.Api.GameAppVersion.Dto;
using GameFrameX.Client.Api.RemoteConfig.Dto;
using GameFrameX.Client.Api.GameAssetPackageVersion.Dto;
using GameFrameX.Client.Api.Integrations.Advertisement;
using GameFrameX.Core.Bootstrap;
using GameFrameX.Core.Cache;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.Service.TenantPackage;
using GameFrameX.Foundation.Extensions;
using GameFrameX.Foundation.Options;
using Serilog;
using SqlSugar;

namespace GameFrameX.Client.Api;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        // GFX-873 / C381 信号③：进程级未处理异常兜底（后台线程致命异常 / 主线程崩溃打印阶段），须早于任何可能崩溃的构建代码
        CrashExitWatchdog.EnableDomainUnhandledExceptionFallback();

        var builder = WebApplication.CreateBuilder(args);
        // builder.WebHost.UseUrls("http://0.0.0.0:20808");

        // 存档整档上行上限（原 Game.Api，C268 合并迁入）：26 分表整档一次上行峰值。
        // 坑 #5：ponytail: 64MB 为保守估算上限，实测大档分布后收紧；
        // 超过此值的上行直接 413，客户端走重试/分包策略
        builder.WebHost.ConfigureKestrel(options => { options.Limits.MaxRequestBodySize = 64 * 1024 * 1024; });

        var launcherOptions = OptionsBuilder.CreateWithDebug<LauncherOptions>(args);

        if (builder.Environment.IsDevelopment())
        {
            launcherOptions.IsConsole = true;
        }

        // 共享启动装配（C261，GFX-668）：日志 / 雪花 / CORS / MVC / HealthChecks / Swagger 收敛至 Core Bootstrap
        builder.UseHostBootstrapLogging(launcherOptions);
        // Grafana Cloud OpenTelemetry（OTLP 三信号：traces/metrics/logs，logs 自动入 Loki）；凭证齐全才启用
        builder.UseHostOpenTelemetry("GameFrameX.Client.Api");
        builder.Services.AddHostCors();

        // DataProtection 密钥环持久化到 data/DataProtection-Keys（容器内挂载卷 /app/data，与 Web/Hub 同口径）
        builder.UseHostDataProtection();

        Console.WriteLine("当前配置的LauncherOptions摘要:");
        Console.WriteLine(JsonHelper.SerializeFormat(new
        {
            launcherOptions.DbType,
            launcherOptions.IsConsole,
            launcherOptions.IsGrafanaLoki,
            GrafanaLokiUrlConfigured = launcherOptions.GrafanaLokiUrl.IsNotNullOrEmpty(),
            ConnectionStringConfigured = launcherOptions.ConnectionString.IsNotNullOrEmpty(),
        }));
        Console.WriteLine("---------------------------------------------------");
        // 配置雪花ID（Foundation Snowflake 紧凑布局 + WorkerId 防撞，C360 / GFX-803）：
        // 读 SnowFlake:WorkerId（环境变量 SnowFlake__WorkerId），缺省兜底 3（Web.Api 配置 1 / Hub 2 / Client 3，
        // 单机全默认互不冲突）；同库实例必须互斥配置，越界 0-7 启动失败
        builder.UseSnowFlake(3);
        Ip2LocationManager.Init();

        builder.Services.AddSingleton<LauncherOptions>(launcherOptions);
        // 租户解析接线（C260 共享扩展）：IHttpContextAccessor + 读取统一键 TenantContextAccessor.TenantIdKey 的解析器一次注册
        var tenantIdResolver = builder.Services.AddSqlSugarTenantResolution(new HttpContextAccessor());
        builder.Services.AddHttpClient();
        // 共享 MVC 装配（请求/响应日志两件套 + PascalCase JSON，C261 收敛至 Core）；
        // Client 特有：登录阻断异常（处罚事实源命中）→ HttpJsonResultData<BaseOperationAckResponse>.Error（不进共享层）
        builder.Services.AddHostControllers(option => { option.Filters.Add<PenaltyLoginBlockedExceptionFilter>(); });

        // 主库连接统一走 DbConnection 节 + GFX_MAIN_DB_* 单一入口（C269）；LauncherOptions 裸参数仅作兼容兜底
        var mainDbConfig = HostMainDbConnectionResolver.Resolve(builder.Configuration, launcherOptions);
        var sqlSugar = SqlSugarClientConfig.AddSqlSugarClientService(mainDbConfig, tenantIdResolver);

        builder.Services.AddSingleton<ISqlSugarClient>(sqlSugar); // 单例注册
        builder.Services.AddScoped(typeof(Repository<>)); // 仓储注册
        // 基类服务面：FeatureSwitchDeliveryResolver 等共享实现以 SqlSugarRepository<T> 为构造参数（C345 冻结契约），
        // DI 不做基类向上匹配，须显式注册开放泛型基类 -> Repository<T> 实现（与上行为不同 descriptor，仓储无状态可各自实例化）
        builder.Services.AddScoped(typeof(SqlSugarRepository<>), typeof(Repository<>));
        // Core 服务（如 TenantAccessGate）按基类 SqlSugarRepository<T> 注入；映射到 Client.Api 的 Repository<T>，
        // 其构造已用 ClientApiTenantDbProvider 正确接入主库 / 租户库 Context，避免基类回退无参构造导致 Context 为空、查询 NRE。
        builder.Services.AddScoped(typeof(SqlSugarRepository<>), typeof(Repository<>));
        // 租户访问门禁依赖链：TenantExpirationMiddleware 解析 TenantAccessGate，进而依赖 ICache / SystemCacheService / TenantExpirationService。
        // 这组 Core 服务历史上未在 Client.Api 注册，导致中间件抛 InvalidOperationException、所有请求（含 /health 探针）返回 500。
        builder.Services.AddCache(); // 注册 ICache（MemoryCacheService 实现）
        // C1 传输签名验证：SignatureValidation 节配置，默认关闭，与客户端同版本上线时打开
        builder.Services.Configure<SignatureValidationOptions>(builder.Configuration.GetSection("SignatureValidation"));
        // C2 签名密钥环：App 级 v2 密钥解析（依赖均为 Singleton，缓存为进程内懒刷新，同 AppCacheService 口径）
        builder.Services.AddSingleton<ISignatureKeyResolver, SignatureKeyResolver>();
        // C2 验签灰度分流：App 级 RiskEnforcementMode（依赖 Scoped 仓储，随请求作用域解析）
        builder.Services.AddScoped<IGameAppRiskEnforcementModeResolver, GameAppRiskEnforcementModeResolver>();
        // C5 加密信封：会话密钥环解析（Singleton + 进程内懒刷新，同 SignatureKeyResolver 口径）
        builder.Services.AddSingleton<ISessionKeyResolver, SessionKeyResolver>();
        // C5 信封灰度配置：默认关闭 = 完全旁路（不解密不加密）；CryptoEnvelope:TestTenantIds 命中的测试租户即使打开也旁路
        builder.Services.Configure<CryptoEnvelopeOptions>(builder.Configuration.GetSection("CryptoEnvelope"));
        // C5 协议版本白名单配置（缺头拒绝；未知版本 AuditOnly 仅告警放行）
        builder.Services.Configure<ProtoVersionOptions>(builder.Configuration.GetSection("ProtoVersion"));
        builder.Services.AddSingleton<SystemCacheService>(); // 仅依赖 ICache + IOptions<CacheOptions>
        builder.Services.AddScoped<TenantExpirationService>(); // 依赖 SqlSugar 仓储
        builder.Services.AddScoped<TenantAccessGate>(); // 依赖 SystemCacheService + 仓储 + TenantExpirationService
        builder.Services.AddTransient<TenantGameOrderShipService>(); // 订单发货/退款服务
        builder.Services.AddSingleton<ClientApiTenantDbProvider>(); // 当前请求 TenantId -> 租户业务库连接
        builder.Services.AddSingleton<ITenantDbProvider>(sp => sp.GetRequiredService<ClientApiTenantDbProvider>()); // 共享层密钥环 Resolver 依赖接口
        builder.Services.AddSingleton<GameAppPackageResolver>(); // 包名解析服务
        builder.Services.AddSingleton<AppCacheService>(); // 应用缓存服务
        builder.Services.AddSingleton<PublicCallbackRouteResolver>(); // 公开 provider 回调 URL -> 租户/应用上下文解析
        // Client API 请求期租户配置 Provider：依赖 IHttpContextAccessor + ISqlSugarClient，按请求作用域读取租户库配置
        builder.Services.AddScoped<IClientApiTenantOptionsProvider, ClientApiTenantOptionsProvider>();
        // 广告奖励发放账本服务（C114 / GFX-110）：依赖 Repository<T> + IClientApiTenantOptionsProvider + HttpClient + IHttpContextAccessor
        builder.Services.AddAdvertisementCallbacks();
        builder.Services.AddScoped<GameAdvertisementRewardService>();
        builder.Services.AddScoped<RemoteConfigDeliveryService>();
        // 活动/功能开关投放解析器（C345 共享实现，依赖 Scoped 租户仓储，随请求作用域解析）
        builder.Services.AddScoped<GameFrameX.Application.Game.App.FeatureSwitch.FeatureSwitchDeliveryResolver>();
        // 邮件分域：租户业务邮件（玩家注册 / 找回密码）；平台系统邮件（SystemMessageService 等）走 ISystemEmailSender
        builder.Services.AddScoped<ITenantEmailSender, TenantEmailSender>();
        builder.Services.AddSingleton<Core.Service.Message.ISystemEmailSender,
            GameFrameX.Application.Game.System.Email.SystemEmailSender>();
        // Token / Ticket 签发服务依赖 IClientApiTenantOptionsProvider（Scoped），必须随之降为 Scoped
        builder.Services.AddScoped<ClientApiTokenIssuer>();
        // C7 登录会话密钥签发：依赖 Scoped 租户仓储（SqlSugarRepository<>），随请求作用域
        builder.Services.AddScoped<ClientApiSessionKeyIssuer>();
        builder.Services.AddScoped<IGameLoginTicketKeyResolver, GameLoginTicketKeyResolver>();
        builder.Services.AddScoped<GameLoginTicketIssuer>();
        // 处罚事实源登录阻断查询（BanAsync / RestrictLogin 命中即拦截登录漏斗）
        builder.Services.AddScoped<ITenantGamePenaltyEnforcementService, TenantGamePenaltyEnforcementService>();
        // 玩家账号密码哈希服务（PBKDF2，change C198 / GFX-627）：供 GameUserController 写入 / 校验玩家密码
        builder.Services.AddScoped<TenantGameUserPasswordService>();

        // 注册 Fallback 匹配器
        builder.Services.AddScoped<IFallbackMatcher<GameAppVersionRequest, TenantGameAppReleaseVersionEntity>, AppVersionFallbackMatcher>();
        builder.Services.AddScoped<IFallbackMatcher<RemoteConfigRequest, TenantGameAppRemoteConfigEntity>, RemoteConfigFallbackMatcher>();
        builder.Services.AddScoped<IFallbackMatcher<AssetPackageVersionRequest, TenantGameAppAssetPackageVersionEntity>, AssetPackageVersionFallbackMatcher>();

        builder.Services.AddSerilog();
        builder.Services.AddHostHealthChecks();

        // 注册支付渠道服务
        builder.Services.AddPaymentChannels();

        // C11 T3 支付域（原 Game.Api，C268 合并迁入）：游戏内支付收据验真通道
        // （依赖 AddPaymentChannels 的 IPaymentChannelFactory + IHttpClientFactory，均已具备）
        builder.Services.AddScoped<IPayReceiptVerifier, GamePayReceiptVerifier>();

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
        applicationLifetime.ApplicationStopped.Register(() => CrashExitWatchdog.Arm("GameFrameX.Client.Api host 已停止"));

        app.UseMiddleware<OptimisticConcurrencyMiddleware>();

        if (enableSwagger)
        {
            app.UseHostSwaggerUi();
        }

        // 设置跨域
        app.UseHostCors();
        // 请求语言解析（Accept-Language → CurrentUICulture，错误码文案按请求语言返回）
        app.UseHostRequestLocalization();

        app.UseRouting();

        // 租户解析中间件
        app.UseMiddleware<TenantResolutionMiddleware>();

        // 公开 provider 回调租户解析中间件
        app.UseMiddleware<PublicCallbackTenantResolutionMiddleware>();

        // 协议版本中间件（C5 server-envelope-middleware）：X-Proto-Ver 缺头拒绝，未知版本 AuditOnly 只告警不拦截
        app.UseMiddleware<ProtoVersionMiddleware>();

        // 传输签名验证中间件（C1 hmac-foundation-migration）：公开回调已打标，中间件内部豁免
        app.UseMiddleware<SignatureValidationMiddleware>();

        // 加密信封中间件（C5 server-envelope-middleware）：挂签名之后——签名覆盖线上信封字节，验签先于解密；请求解密、响应加密
        app.UseMiddleware<CryptoEnvelopeMiddleware>();

        // 租户到期拦截中间件
        app.UseMiddleware<TenantExpirationMiddleware>();

        // 应用验证中间件
        app.UseMiddleware<AppResolutionMiddleware>();

        // 启动时加载应用缓存
        var appCacheService = app.Services.GetRequiredService<AppCacheService>();
        await appCacheService.InitializeAsync();

        // 后台定时刷新应用缓存（每 30 秒检查一次）
        _ = Task.Run(async () =>
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
            while (await timer.WaitForNextTickAsync())
            {
                try
                {
                    await appCacheService.CheckAndRefreshAsync();
                }
                catch
                {
                    /* 静默失败，下次重试 */
                }
            }
        });

        // 启动时加载包映射缓存
        var packageResolver = app.Services.GetRequiredService<GameAppPackageResolver>();
        await packageResolver.InitializeAsync();

        // 后台定时刷新包映射（每 30 秒检查一次）
        _ = Task.Run(async () =>
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
            while (await timer.WaitForNextTickAsync())
            {
                try
                {
                    await packageResolver.CheckAndRefreshAsync();
                }
                catch
                {
                    /* 静默失败，下次重试 */
                }
            }
        });

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
            CrashExitWatchdog.Arm("GameFrameX.Client.Api host 未处理异常", exception);
            throw;
        }
    }
}