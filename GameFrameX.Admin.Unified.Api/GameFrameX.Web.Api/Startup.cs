using System.Net;
using System.Threading.RateLimiting;
using GameFrameX.Application.System.Job;
using GameFrameX.Application.Game.Job;
using GameFrameX.Application.Game.ServerManager.Mail;
using GameFrameX.Application.Game.Online.LiveOps.Dispatch;
using GameFrameX.Application.Game.ServerManager.Online;
using GameFrameX.Application.Game.Order.GamePaymentConfig;
using GameFrameX.Advertisement.Abstract;
using GameFrameX.Payment.Abstract;
using GameFrameX.Core.Bootstrap;
using GameFrameX.Core.Cache;
using GameFrameX.Core.Enum.Errors;
using GameFrameX.Core.EventBus;
using GameFrameX.Core.Extension;
using GameFrameX.Core.Filter;
using GameFrameX.Core.Hub;
using GameFrameX.Core.Job;
using GameFrameX.Core.Logging;
using GameFrameX.Core.Provider;
using GameFrameX.Core.Service.Message;
using GameFrameX.Core.Service.PlatformRateLimit;
using GameFrameX.Core.Service.PlatformLogging;
using GameFrameX.Core.Service.PlatformPasswordPolicy;
using GameFrameX.Core.Service.TenantMenuSync;
using GameFrameX.Core.SignalR;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Utility;
using GameFrameX.Core.Service.PlatformCaptcha;
using GameFrameX.ObjectStorage.Abstract;
using GameFrameX.Web.Api.ApiRouting;
using GameFrameX.Web.Api.DependencyInjection;
using GameFrameX.Web.Api.Handlers;
using Lazy.Captcha.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using GameFrameX.ObjectStorage.Local;

namespace GameFrameX.Web.Api;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var configuration = ResolveStartupService<IConfiguration>(services);
        var webHostEnvironment = ResolveStartupService<IWebHostEnvironment>(services);
        var enableSchedule = Environment.GetEnvironmentVariable("GFX_DISABLE_SCHEDULE") != "1";
        var httpContextAccessor = new HttpContextAccessor();
        services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);
        CommonUtility.Configure(httpContextAccessor);

        // 配置选项
        services.AddProjectOptions();
        services.AddProjectDependencyInjection();
        // 密码哈希统一 Argon2id（C214），无需启动期密钥注入；密码强度策略由 PlatformPasswordPolicyConfigProvider 快照驱动。

        // 缓存注册
        var cache = services.AddCache();
        LocalObjectStorageAdapter.Configure(webHostEnvironment);
        // SqlSugar
        services.AddSqlSugar(configuration, webHostEnvironment, cache, httpContextAccessor);
        // Admin 侧租户库连接提供器：Core 收敛层（Repository 租户表路由 / 签名密钥环 Resolver）依赖，与 Client.Api / Game.Api 的注册同口径
        services.AddSingleton<ITenantDbProvider, AdminTenantDbProvider>();
        // 支付配置 schema 提供器（供 TenantGamePaymentConfigEntityService 在 admin 主机解析）
        services.AddScoped<IPaymentConfigSchemaProvider, PaymentConfigSchemaProvider>();
        // 渠道退款管理器（C310 / GFX-718）：四渠道统一 IRefundManager 契约；Payment.* 工程不在自动扫描范围，须显式注册
        services.AddScoped<IRefundManager, GameFrameX.Payment.Xsolla.Refund.XsollaRefundManager>();
        services.AddScoped<IRefundManager, GameFrameX.Payment.Apple.Refund.AppleRefundManager>();
        services.AddScoped<IRefundManager, GameFrameX.Payment.Google.Refund.GoogleRefundManager>();
        services.AddScoped<IRefundManager, GameFrameX.Payment.AliPay.Refund.AliPayRefundManager>();
        // 广告回调配置 schema 提供器（供 TenantAdvertisementCallbackConfigEntityService 动态渲染和校验）
        services.AddScoped<IAdvertisementCallbackConfigSchemaProvider, AdvertisementCallbackConfigSchemaProvider>();
        // 平台系统邮件发送器（ISystemEmailSender 实现位于 Application.Game；从主库 SystemEmailProviderEntity 读取）
        services.AddSingleton<ISystemEmailSender, GameFrameX.Application.Game.System.Email.SystemEmailSender>();
        // 对象存储配置 schema 提供器（供 TenantObjectStorageConfigEntityService 动态渲染和校验）
        services.AddScoped<IObjectStorageConfigSchemaProvider, ObjectStorageConfigSchemaProvider>();
        // 对象存储 Adapter 工厂（单例；供 TenantFileService 与 TestConnectionAsync 端点解析租户存储配置）
        services.AddSingleton<IObjectStorageAdapterFactory, ObjectStorage.Abstract.DefaultObjectStorageAdapterFactory>();
        services.AddHttpClient();
        services.AddHttpClient<IMailCampaignServerClient, MailCampaignServerClient>();
        // C339 / vault:C2 S1.7：Online 平台管理客户端基座（作用域注入 + 镜像错误码映射）。
        services.AddScoped<OnlineScopeContextFactory>();
        services.AddScoped<IOnlineServerEndpointResolver, OnlineServerEndpointResolver>();
        services.AddHttpClient<IOnlineServerClient, OnlineServerClient>();
        // C343 / vault:C8 S7.7：LiveOps 统一下发单一入口（业务层唯一消费 IOnlineServerClient 的调度面）。
        services.AddScoped<IOnlineLiveOpsDispatchService, OnlineLiveOpsDispatchService>();
        // C200：外网 IP 查询命名客户端（ComputerUtility.GetIpFromOnlineAsync 使用），配置超时避免国外部署 myip.ipip.net 挂死。
        services.AddHttpClient("remoteIp", client => { client.Timeout = TimeSpan.FromSeconds(5); });
        services.AddSingleton<PlatformRateLimitConfigProvider>();
        services.AddHostedService<PlatformRateLimitStartupLoader>();
        // 平台日志运行时配置（C171）：快照由启动加载器填充，DB/ES Writer 写路径读快照实现同 Host 动态生效
        services.AddSingleton<PlatformLoggingConfigProvider>();
        services.AddHostedService<PlatformLoggingStartupLoader>();
        // 平台密码策略运行时配置（C173）：快照由启动加载器填充并镜像同步 CryptogramUtility 密码策略静态属性，
        // TenantUserService.ChangePwdAsync 读镜像实现保存后同 Host 动态生效
        services.AddSingleton<PlatformPasswordPolicyConfigProvider>();
        services.AddHostedService<PlatformPasswordPolicyStartupLoader>();
        // JWT
        services.AddHostedService<PlatformJwtStartupValidator>();
        // U6/C42：API 端点目录启动同步（端点登记→绑定迁移→自动隐藏按钮→预热权限缓存）。
        services.AddHostedService<ApiEndpointCatalogSync>();
        // C353 / GFX-773：套餐驱动的存量租户菜单权限自动同步（队列单例 + Applier 作用域 + 后台工作器）。
        // 注册顺序在 ApiEndpointCatalogSync 之后：Worker 启动扫描有目录就绪等待兜底，不依赖严格启动序。
        services.AddSingleton<TenantMenuSyncQueue>();
        services.AddScoped<TenantMenuSyncApplier>();
        services.AddHostedService<TenantMenuSyncWorker>();
        services.AddPlatformJwt();
        // 允许跨域（支持通过 CORS_ORIGINS 环境变量配置允许的域名列表，逗号分隔；未配置则允许所有来源）
        var corsOrigins = Environment.GetEnvironmentVariable("CORS_ORIGINS");
        if (!string.IsNullOrWhiteSpace(corsOrigins))
        {
            var origins = corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(o => o.Trim())
                                     .Where(o => !string.IsNullOrWhiteSpace(o))
                                     .ToArray();
            if (origins.Length > 0)
            {
                services.AddCors(options =>
                {
                    options.AddDefaultPolicy(policy => policy.AllowAnyHeader()
                                                             .AllowAnyMethod()
                                                             .WithOrigins(origins)
                                                             // 跨域默认读不到 Content-Disposition，导出文件名会丢（getFileName 崩溃）
                                                             .WithExposedHeaders("Content-Disposition")
                                                             .AllowCredentials());
                });
            }
        }
        else
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy => policy.AllowAnyHeader()
                                                         .AllowAnyMethod()
                                                         .SetIsOriginAllowed(_ => true)
                                                         .WithExposedHeaders("Content-Disposition")
                                                         .AllowCredentials());
            });
        }

        // 共享健康检查注册（C261，GFX-668）：与三 host 同口径收敛至 Core Bootstrap（self live + database ready）
        services.AddHostHealthChecks();
        if (enableSchedule)
        {
            services.AddAdminSchedule(true);
            services.AddGameSchedule();
            services.AddSingleton<ScheduledTaskHostedService>();
            services.AddHostedService(sp => sp.GetRequiredService<ScheduledTaskHostedService>());
        }
        else
        {
            services.AddAdminSchedule(false);
            services.AddGameSchedule();
        }

        services.AddControllersWithViews()
                .AddAdminServiceControllers()
                .AddJsonOptions(options => JsonSerializerOptionsProvider.Configure(options.JsonSerializerOptions))
                //.AddXmlSerializerFormatters()
                //.AddXmlDataContractSerializerFormatters()
                .AddMvcOptions(options =>
                {
                    // 全局授权：所有 MVC action 默认需登录，[AllowAnonymous] 放行。
                    // 取代 FallbackPolicy —— FallbackPolicy 连 Swagger 中间件路径、404 路径都拦截，
                    // 是 Swagger 返回 401 的根因；AuthorizeFilter 只作用于 MVC action，文档中间件天然匿名。
                    options.Filters.Add(new AuthorizeFilter());
                    options.Filters.AddService<TenantExpirationFilter>();
                    options.Filters.Add<AdminResultFilter>();
                });
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState.Where(item => item.Value?.Errors.Count > 0)
                                    .ToDictionary(item => item.Key,
                                                  item => item.Value!.Errors.Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? error.Exception?.Message : error.ErrorMessage).ToArray());
                return new JsonResult(AdminResultFactory.CreateError(StatusCodes.Status400BadRequest, errors, (int)CommonErrorCode.ParameterValidationFailed))
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                };
            };
        });
        AddAdminSwagger(services);

        // ElasticSearch（C171：配置由 PlatformLoggingConfigProvider 快照驱动，不再读 Logging.json）
        services.AddElasticSearch();

        // 配置Nginx转发获取客户端真实IP。
        // 默认只信任框架内置的 Loopback 代理；如需信任其他代理，请配置明确的地址或网段。
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            ConfigureKnownForwardedHeaderProxies(options);
        });

        // 平台限流服务
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = 429;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var settings = context.RequestServices.GetRequiredService<PlatformRateLimitConfigProvider>().GetSnapshot();
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var partitionKey = PlatformRateLimitConfigProvider.BuildPartitionKey(settings, ip);
                if (!settings.IsEnabled)
                {
                    return RateLimitPartition.GetNoLimiter(partitionKey);
                }

                return RateLimitPartition.GetFixedWindowLimiter(partitionKey,
                                                                _ => new FixedWindowRateLimiterOptions
                                                                {
                                                                    PermitLimit = settings.PermitLimit,
                                                                    Window = TimeSpan.FromSeconds(settings.WindowSeconds),
                                                                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                                                    QueueLimit = settings.QueueLimit,
                                                                });
            });
        });

        // 事件总线
        services.AddEventBus(options =>
        {
            options.UseUtcTimestamp = false;
            // 不启用事件日志
            options.LogEnabled = false;
            // 事件执行器（失败重试）
            options.AddExecutor<RetryEventHandlerExecutor>();
            //// 替换事件源存储器
            //options.ReplaceStorer(serviceProvider =>
            //{
            //    var redisCache = serviceProvider.GetService<ICache>();
            //    // 创建默认内存通道事件源对象，可自定义队列路由key，比如这里是 eventbus
            //    return new RedisEventSourceStorer(redisCache, "eventbus", 3000);
            //});
        });

        // 平台系统邮件只从主库 SystemEmailProviderEntity 读取；缺配置时明确失败，不启用静态 SMTP fallback。

        // 即时通讯
        services.AddProjectSignalR();

        // 系统日志（C171：DB/ES Writer 无条件注册，启停由快照动态门控，不再读 Logging.json）
        services.AddLoggingSetup();

        // 验证码：配置从主库 system_config 读取，运行时缓存由保存操作失效。
        services.AddSingleton<PlatformCaptchaStorage>();
        services.AddTransient<ICaptcha, PlatformCaptchaService>();

        services.AddSingleton<AdminResultFilter>();

        // 控制台logo
        services.AddConsoleLogo();
    }

    private static void ConfigureKnownForwardedHeaderProxies(ForwardedHeadersOptions options)
    {
        AddKnownProxies(options, Environment.GetEnvironmentVariable("GFX_FORWARDED_KNOWN_PROXIES"));
        AddKnownNetworks(options, Environment.GetEnvironmentVariable("GFX_FORWARDED_KNOWN_NETWORKS"));
    }

    private static void AddKnownProxies(ForwardedHeadersOptions options, string? proxies)
    {
        if (string.IsNullOrWhiteSpace(proxies))
        {
            return;
        }

        foreach (var proxy in proxies.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (IPAddress.TryParse(proxy, out var ipAddress))
            {
                options.KnownProxies.Add(ipAddress);
            }
        }
    }

    private static void AddKnownNetworks(ForwardedHeadersOptions options, string? networks)
    {
        if (string.IsNullOrWhiteSpace(networks))
        {
            return;
        }

        foreach (var network in networks.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (System.Net.IPNetwork.TryParse(network, out var ipNetwork))
            {
                options.KnownIPNetworks.Add(ipNetwork);
            }
        }
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
        app.UseMiddleware<OptimisticConcurrencyMiddleware>();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseForwardedHeaders();
        }
        else
        {
            app.UseForwardedHeaders();
            app.UseHsts();
        }

        //// 启用HTTPS
        //app.UseHttpsRedirection();

        // 特定文件类型（文件后缀）处理
        var contentTypeProvider = new FileExtensionContentTypeProvider();
        // contentTypeProvider.Mappings[".文件后缀"] = "MIME 类型";
        app.UseStaticFiles(new StaticFileOptions
        {
            ContentTypeProvider = contentTypeProvider,
        });

        // 请求语言解析（Accept-Language → CurrentUICulture，错误码文案按请求语言返回）
        app.UseHostRequestLocalization();

        app.UseRouting();

        app.UseCors();

        // 限流中间件
        app.UseRateLimiter();

        app.UseAuthentication();
        app.UseAuthorization();

        // 任务调度看板
        app.UseScheduleUI();

        // Swagger 暴露：开发环境默认开启；生产/Docker 通过配置 Swagger:IsEnabled（环境变量 Swagger__Enabled）显式开启，
        // 兼顾“Docker 内可访问”与“生产默认不外泄文档”。三个 Api 项目各自独立 Swagger，互不聚合。
        var enableSwagger = env.IsHostSwaggerEnabled(configuration);
        if (enableSwagger)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = "swagger";
                options.SwaggerEndpoint("/" + AdminOpenApiGroups.WebGroup.RouteTemplate, AdminOpenApiGroups.WebGroup.Title);
            });
        }

        app.UseEndpoints(endpoints =>
        {
            // 健康探针必须匿名放行：存活/就绪探针用于容器编排与负载均衡，不能依赖登录态。
            // 否则 JwtHandler 对未登记端点 fail-closed（JwtHandler.CheckAuthorizeAsync），/health 返回 401，
            // docker healthcheck 期望 200 会判定 unhealthy，进而阻塞 depends_on: service_healthy 的下游服务。
            // 端点映射与三 host 共用 Core Bootstrap（C261），Admin 特有要求：全局鉴权下必须 allowAnonymous。
            endpoints.MapHostHealthChecks(allowAnonymous: true);

            // 注册动态 API controller 端点。此前靠 c47ff90d 误删的 MapControllerRoute("default") 顺带注册
            // attribute-routed controllers，删除后全部 /api/** 404（容器旧镜像仍含旧路由，回归未暴露）。
            // 动态 API 全部 attribute route，用 MapControllers 精确注册，不再恢复 MVC 默认视图路由。
            endpoints.MapControllers();

            // 注册集线器
            // SignalR Hub 端点显式要求授权。移除全局 FallbackPolicy 后 Hub 不再被兜底保护，
            // 此处补 RequireAuthorization 维持"需登录"语义、避免匿名裸奔（与移除前 401 行为一致；
            // 完整的 SignalR query token 认证为独立后续任务，不在本次根治 Swagger 范围内）。
            endpoints.MapHub<OnlineUserHub>(OnlineUserHub.HubPath).RequireAuthorization();
        });
    }

    private static T ResolveStartupService<T>(IServiceCollection services) where T : class
    {
        var descriptor = services.LastOrDefault(item => item.ServiceType == typeof(T));
        if (descriptor?.ImplementationInstance is T instance)
        {
            return instance;
        }

        // ponytail: Startup 仍保留无参激活路径；若启动期改为构造注入，删除这个兜底。
        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<T>();
    }

    private static void AddAdminSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            // 收敛为单个 web.api 文档：不再按业务模块拆 SwaggerDoc，模块分类改由 Swagger Tag（左侧菜单）承担。
            // DocInclusionPredicate 收所有 IAdminApiController，忽略各 Controller 上残留的 GroupName。
            options.SwaggerDoc(AdminOpenApiGroups.Web, new OpenApiInfo { Title = AdminOpenApiGroups.WebGroup.Title, Version = "v1", });
            options.DocInclusionPredicate((documentName, apiDescription) => string.Equals(documentName, AdminOpenApiGroups.Web, StringComparison.OrdinalIgnoreCase));

            var directoryInfo = new DirectoryInfo(AppContext.BaseDirectory);
            foreach (var fileInfo in directoryInfo.GetFiles("*.xml", SearchOption.AllDirectories))
            {
                options.IncludeXmlComments(fileInfo.FullName, true);
            }

            options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            options.OrderActionsBy(apiDescription => apiDescription.RelativePath);
            // 让 schema 反映 AdminResultFilter 的运行时包装（AdminResult<T>），并补全 operationId。
            // 否则 schema 只反映方法签名返回类型 T，与实际 {code,result,...} 响应脱节，前端按 openapi 生成的客户端类型错误。
            options.OperationFilter<AdminResultOperationFilter>();
            // 让 schema 与 ProjectJsonSerializerOptions 的 long→string 运行时契约一致：
            // Id 结尾的 long 响应实际输出字符串，schema 同步声明 string，防止前端生成 number 类型诱导精度回转（C270）。
            options.SchemaFilter<LongIdentifierStringSchemaFilter>();
            // 为整数枚举 schema 补 x-enum-varnames / x-enum-descriptions：swagger-typescript-api 依赖它生成
            // 命名枚举成员（否则退化为 Value1/Value2，前端无法按语义引用）。
            options.SchemaFilter<EnumMemberNamesSchemaFilter>();
        });
    }
}