using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Nodes;
using GameFrameX.Core.Cache;
using GameFrameX.Core.Const;
using GameFrameX.Core.EventBus;
using GameFrameX.Core.Extension.Cache;
using GameFrameX.Core.Logging;
using GameFrameX.Core.Option;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Config;
using GameFrameX.Core.Service.PlatformLogging;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.System;
using GameFrameX.Entity;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Log;
using GameFrameX.Entity.System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace GameFrameX.Core.Tests.Logging;

/// <summary>
/// Monitor 日志采集与调度管线回归守护测试（C363 / T5，真实 HTTP → 行落库）：
/// TestServer 挂真实 <see cref="LoggingSetup.AddLoggingSetup"/>（Monitor 过滤器 + 调度队列 + 文件/数据库/ES Sink），
/// SqlSugar sqlite 主库（main）+ 租户库（1001）双连接，覆盖三表分流、C156 分表路由、C171 动态开关、
/// sys_oplog 门控、[SuppressMonitor] 豁免、数据校验短路与 data/logs 文件生成。防复发锚点：
/// 历史框架退役（2026-07-27）后全库 *_log_* 表零写入的事故不允许再次静默发生。
/// </summary>
public sealed class MonitorLoggingPipelineTests : IAsyncLifetime
{
    private const string MainConfigId = "main";
    private const string TenantConfigId = "1001";
    private const string SuperAdminIdentity = "superadmin";
    private const string TenantIdentity = "tenant";
    private const string AnonymousIdentity = "anonymous";
    private const string TestSchemaName = "TestScheme";

    private readonly string _contentRoot;
    private readonly List<string> _dbPaths = new();
    private SqlSugarScope _db = null!;
    private IHost _host = null!;

    public MonitorLoggingPipelineTests()
    {
        _contentRoot = Path.Combine(Path.GetTempPath(), $"gfx_monitor_pipeline_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_contentRoot);
    }

    public async Task InitializeAsync()
    {
        SqlSugarCache.Configure(new FakeCache());
        var mainConfig = CreateDbConfig(MainConfigId);
        var tenantConfig = CreateDbConfig(TenantConfigId);
        _db = new SqlSugarScope(new List<ConnectionConfig> { mainConfig, tenantConfig, });
        SqlSugarSetup.InitDatabase(_db, mainConfig, new[]
        {
            typeof(SystemLogOperationEntity),
            typeof(SystemLogVisitEntity),
            typeof(SystemLogExceptionEntity),
            typeof(SystemTenantEntity),
            typeof(SystemConfigEntity),
        });
        SqlSugarSetup.InitDatabase(_db, tenantConfig, new[]
        {
            typeof(TenantLogOperationEntity),
            typeof(TenantLogVisitEntity),
            typeof(TenantLogExceptionEntity),
        });
        await SeedSysOpLogEnabledAsync();

        _host = await BuildHostAsync();
    }

    public async Task DisposeAsync()
    {
        using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
        {
            try
            {
                await _host.StopAsync(cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // 宿主停止超时不阻断测试清理
            }

            _host.Dispose();
        }

        foreach (var dbPath in _dbPaths)
        {
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }

        if (Directory.Exists(_contentRoot))
        {
            Directory.Delete(_contentRoot, recursive: true);
        }
    }

    /// <summary>
    /// 验收 1/4/6：平台上下文（SuperAdmin）真实 HTTP 调用后，主库操作日志行三要素（CreatedId/Account、
    /// ControllerName/ActionName/RequestUrl、LogDateTime + RequestParam）齐备且字段面完整。
    /// </summary>
    [Fact]
    public async Task Page_WithSuperAdminIdentity_WritesCompleteOperationRowToMainDatabase()
    {
        var statusCode = await PostAsync("/api/monitorPipeline/page", SuperAdminIdentity, "{\"keyword\":\"alpha\",\"page\":2}");

        Assert.Equal(HttpStatusCode.OK, statusCode);
        var mainDb = _db.GetConnectionScope(MainConfigId);
        var rows = await mainDb.Queryable<SystemLogOperationEntity>()
            .Where(u => u.RequestUrl.Contains("/api/monitorPipeline/page"))
            .ToListAsync();

        var row = Assert.Single(rows);
        // 三要素之一：操作者
        Assert.Equal(1L, row.CreatedId);
        Assert.Equal("superadmin", row.Account);
        Assert.Equal("平台管理员", row.RealName);
        // 三要素之二：调用面
        Assert.Equal("MonitorPipelineTest", row.ControllerName);
        Assert.Equal("GetPage", row.ActionName);
        Assert.Equal("Monitor管线分页", row.DisplayTitle);
        Assert.Equal("POST", row.HttpMethod);
        // 三要素之三：时间 + 参数
        Assert.True(row.LogDateTime > 0, "LogDateTime 必须写入（秒级时间戳）");
        Assert.Contains("alpha", row.RequestParam);
        // 字段面：完整报文与返回值快照（Message 即 loggingMonitor JSON 原文）
        Assert.NotNull(row.Message);
        Assert.Contains("MonitorPipelineTest", row.Message);
        Assert.Contains("alpha", row.Message);
        Assert.NotNull(row.ReturnResult);
        Assert.NotNull(row.TraceId);
        Assert.Equal(LogLevel.Information, row.LogLevel);
        Assert.Null(row.Exception);
        // 三表分流：同一调用不得再落 visit / exception
        Assert.Empty(await mainDb.Queryable<SystemLogVisitEntity>().Where(u => u.ActionName == "GetPage").ToListAsync());
        Assert.Empty(await mainDb.Queryable<SystemLogExceptionEntity>().Where(u => u.RequestUrl.Contains("/api/monitorPipeline/page")).ToListAsync());
    }

    /// <summary>
    /// 验收 5（C156）：租户上下文操作日志路由到租户库 TenantLog*，主库不落任何该调用行。
    /// </summary>
    [Fact]
    public async Task Page_WithTenantIdentity_WritesOperationRowToTenantDatabase()
    {
        var statusCode = await PostAsync("/api/monitorPipeline/page", TenantIdentity, "{\"keyword\":\"tenant-query\"}");

        Assert.Equal(HttpStatusCode.OK, statusCode);
        var tenantDb = _db.GetConnectionScope(TenantConfigId);
        var tenantRows = await tenantDb.Queryable<TenantLogOperationEntity>()
            .Where(u => u.RequestUrl.Contains("/api/monitorPipeline/page"))
            .ToListAsync();

        var row = Assert.Single(tenantRows);
        Assert.Equal(1001L, row.TenantId);
        Assert.Equal("tenant-admin", row.Account);
        Assert.Equal("GetPage", row.ActionName);
        Assert.Contains("tenant-query", row.RequestParam);

        var mainDb = _db.GetConnectionScope(MainConfigId);
        Assert.Empty(await mainDb.Queryable<SystemLogOperationEntity>().Where(u => u.RequestUrl.Contains("/api/monitorPipeline/page")).ToListAsync());
        Assert.Empty(await tenantDb.Queryable<TenantLogVisitEntity>().Where(u => u.ActionName == "GetPage").ToListAsync());
    }

    /// <summary>
    /// 验收 2/3：失败调用以 Error 级进 exception 表，Exception 字段非空，并触发异常邮件事件。
    /// </summary>
    [Fact]
    public async Task FailCall_WritesExceptionRowWithExceptionFilled()
    {
        var statusCode = await PostAsync("/api/monitorPipeline/fail", AnonymousIdentity, "{}");

        Assert.Equal(HttpStatusCode.InternalServerError, statusCode);
        var mainDb = _db.GetConnectionScope(MainConfigId);
        var exceptionRows = await mainDb.Queryable<SystemLogExceptionEntity>()
            .Where(u => u.RequestUrl.Contains("/api/monitorPipeline/fail"))
            .ToListAsync();

        var row = Assert.Single(exceptionRows);
        Assert.NotNull(row.Exception);
        Assert.Contains("monitor-pipeline-fail", row.Exception);
        Assert.Equal(LogLevel.Error, row.LogLevel);
        Assert.Equal("MonitorPipelineTest", row.ControllerName);

        // 异常路径不得再落 operation / visit
        Assert.Empty(await mainDb.Queryable<SystemLogOperationEntity>().Where(u => u.RequestUrl.Contains("/api/monitorPipeline/fail")).ToListAsync());
        Assert.Empty(await mainDb.Queryable<SystemLogVisitEntity>().Where(u => u.ActionName == "Fail").ToListAsync());

        var publisher = (CaptureEventPublisher)_host.Services.GetRequiredService<IEventPublisher>();
        Assert.Contains("Send:ErrorMail", publisher.PublishedEventIds);
    }

    /// <summary>
    /// 验收 4（lesson 2026-07-11）：userInfo/logout 路由段仅落 visit 表（按上下文分平台/租户库），不落 operation。
    /// </summary>
    [Fact]
    public async Task UserInfoAndLogout_WriteVisitRowsOnly()
    {
        var userInfoStatus = await PostAsync("/api/monitorPipeline/userInfo", TenantIdentity, "{}");
        var logoutStatus = await PostAsync("/api/monitorPipeline/logout", AnonymousIdentity, "{}");

        Assert.Equal(HttpStatusCode.OK, userInfoStatus);
        Assert.Equal(HttpStatusCode.OK, logoutStatus);

        var tenantDb = _db.GetConnectionScope(TenantConfigId);
        var tenantVisitRows = await tenantDb.Queryable<TenantLogVisitEntity>().Where(u => u.ActionName == "GetUserInfo").ToListAsync();
        Assert.Single(tenantVisitRows);
        Assert.Equal(1001L, tenantVisitRows[0].TenantId);
        Assert.Equal("tenant-admin", tenantVisitRows[0].Account);

        var mainDb = _db.GetConnectionScope(MainConfigId);
        var mainVisitRows = await mainDb.Queryable<SystemLogVisitEntity>().Where(u => u.ActionName == "Logout").ToListAsync();
        Assert.Single(mainVisitRows);
        Assert.True(mainVisitRows[0].LogDateTime > 0, "visit 行 LogDateTime 必须写入");

        // visit 不落 operation / exception
        Assert.Empty(await tenantDb.Queryable<TenantLogOperationEntity>().Where(u => u.RequestUrl.Contains("/api/monitorPipeline/userInfo")).ToListAsync());
        Assert.Empty(await mainDb.Queryable<SystemLogOperationEntity>().Where(u => u.RequestUrl.Contains("/api/monitorPipeline/logout")).ToListAsync());
    }

    /// <summary>
    /// 验收 1 豁免面：方法级 [SuppressMonitor] 整条跳过，三库零落库。
    /// </summary>
    [Fact]
    public async Task SuppressMonitor_DoesNotWriteAnyRow()
    {
        var statusCode = await PostAsync("/api/monitorPipeline/suppressed", SuperAdminIdentity, "{}");

        Assert.Equal(HttpStatusCode.OK, statusCode);
        var mainDb = _db.GetConnectionScope(MainConfigId);
        var tenantDb = _db.GetConnectionScope(TenantConfigId);
        Assert.Empty(await mainDb.Queryable<SystemLogOperationEntity>().Where(u => u.RequestUrl.Contains("/api/monitorPipeline/suppressed")).ToListAsync());
        Assert.Empty(await mainDb.Queryable<SystemLogVisitEntity>().Where(u => u.ActionName == "Suppressed").ToListAsync());
        Assert.Empty(await mainDb.Queryable<SystemLogExceptionEntity>().Where(u => u.RequestUrl.Contains("/api/monitorPipeline/suppressed")).ToListAsync());
        Assert.Empty(await tenantDb.Queryable<TenantLogOperationEntity>().ToListAsync());
    }

    /// <summary>
    /// 数据校验失败（[ApiController] 自动 400）：validation 非空 → 不落任何日志表。
    /// </summary>
    [Fact]
    public async Task InvalidModel_DoesNotWriteAnyRow()
    {
        var statusCode = await PostAsync("/api/monitorPipeline/invalid", SuperAdminIdentity, "{}");

        Assert.Equal(HttpStatusCode.BadRequest, statusCode);
        var mainDb = _db.GetConnectionScope(MainConfigId);
        Assert.Empty(await mainDb.Queryable<SystemLogOperationEntity>().Where(u => u.RequestUrl.Contains("/api/monitorPipeline/invalid")).ToListAsync());
        Assert.Empty(await mainDb.Queryable<SystemLogVisitEntity>().Where(u => u.ActionName == "Invalid").ToListAsync());
        Assert.Empty(await mainDb.Queryable<SystemLogExceptionEntity>().Where(u => u.RequestUrl.Contains("/api/monitorPipeline/invalid")).ToListAsync());
    }

    /// <summary>
    /// 验收 7（C171）：platform_logging_config database.enabled=false 保存 + 刷新快照后，
    /// 同 Host 内后续调用立即停止落库（无租户上下文 → 平台库的既有语义同时得到验证）。
    /// </summary>
    [Fact]
    public async Task DatabaseSwitchDisabled_StopsWritingImmediately()
    {
        // 基线：无租户上下文调用 → 主库平台操作日志行存在（平台兜底路由）
        var baselineStatus = await PostAsync("/api/monitorPipeline/page", AnonymousIdentity, "{\"keyword\":\"before-switch\"}");
        Assert.Equal(HttpStatusCode.OK, baselineStatus);
        var mainDb = _db.GetConnectionScope(MainConfigId);
        Assert.Single(await mainDb.Queryable<SystemLogOperationEntity>().Where(u => u.RequestUrl.Contains("/api/monitorPipeline/page")).ToListAsync());

        // 关停数据库日志：刷新快照使配置行落库，再改值并刷新
        var provider = _host.Services.GetRequiredService<PlatformLoggingConfigProvider>();
        await provider.RefreshAsync();
        var configRow = await mainDb.Queryable<SystemConfigEntity>()
            .FirstAsync(u => u.Code == PlatformLoggingConfigConstants.ConfigCode);
        Assert.NotNull(configRow);
        var settings = PlatformLoggingConfigProvider.CreateDefaultSettings();
        settings.Database.IsEnabled = false;
        configRow.Value = PlatformLoggingConfigProvider.Serialize(settings);
        await mainDb.Updateable(configRow).ExecuteCommandAsync();
        await provider.RefreshAsync();

        var afterSwitchStatus = await PostAsync("/api/monitorPipeline/page", AnonymousIdentity, "{\"keyword\":\"after-switch\"}");
        Assert.Equal(HttpStatusCode.OK, afterSwitchStatus);
        await WaitDispatchDrainedAsync();
        var rows = await mainDb.Queryable<SystemLogOperationEntity>().Where(u => u.RequestUrl.Contains("/api/monitorPipeline/page")).ToListAsync();
        Assert.Single(rows);
        Assert.Contains("before-switch", rows[0].RequestParam);
    }

    /// <summary>
    /// 验收 8：data/logs 按天 + 级别生成 JSON 行文件，Monitor 消息行携带完整 loggingMonitor JSON。
    /// </summary>
    [Fact]
    public async Task FileLogging_WritesMonitorJsonLinesPerDayAndLevel()
    {
        await PostAsync("/api/monitorPipeline/page", AnonymousIdentity, "{\"keyword\":\"file-line\"}");
        await PostAsync("/api/monitorPipeline/fail", AnonymousIdentity, "{}");

        var logsDirectory = Path.Combine(_contentRoot, "data", "logs");
        var informationFile = Path.Combine(logsDirectory, $"{DateTime.Now:yyyyMMdd}_Information.log");
        var errorFile = Path.Combine(logsDirectory, $"{DateTime.Now:yyyyMMdd}_Error.log");
        Assert.True(File.Exists(informationFile), $"缺少 Information 文件日志：{informationFile}");
        Assert.True(File.Exists(errorFile), $"缺少 Error 文件日志：{errorFile}");

        var informationMonitor = ParseFirstMonitorLine(informationFile);
        Assert.Equal("MonitorPipelineTest", (string?)informationMonitor?["controllerName"]);
        Assert.Equal("page", (string?)informationMonitor?["actionName"]);
        Assert.Equal("GetPage", (string?)informationMonitor?["actionTypeName"]);
        Assert.NotNull(informationMonitor?["parameters"]);

        var errorMonitor = ParseFirstMonitorLine(errorFile);
        Assert.Equal("monitor-pipeline-fail", (string?)errorMonitor?["exception"]?["message"]);
    }

    private static JsonNode? ParseFirstMonitorLine(string fileName)
    {
        var content = File.ReadAllText(fileName);
        var firstLine = content.Split('\n', 2)[0].Trim();
        return JsonNode.Parse(firstLine);
    }

    private async Task SeedSysOpLogEnabledAsync()
    {
        var mainDb = _db.GetConnectionScope(MainConfigId);
        await mainDb.Insertable(new SystemConfigEntity
        {
            // 测试库无雪花 AOP：显式 Id，避免与 RefreshAsync 默认插入行（Id=0）冲突
            Id = 910001L,
            Name = "操作日志",
            Code = CommonConst.SysOpLog,
            Value = "True",
            SysFlag = YesNoEnum.Y,
            GroupCode = "Log",
            OrderNo = 50,
            Remark = "Monitor 管线回归测试 seed",
        }).ExecuteCommandAsync();
    }

    private async Task<IHost> BuildHostAsync()
    {
        var host = new HostBuilder()
            .UseContentRoot(_contentRoot)
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddControllers().AddApplicationPart(typeof(MonitorPipelineTestController).Assembly);
                    // 模拟 AdminResultFilter 的生产拓扑：异常过滤器消化未处理异常并渲染 500，
                    // Monitor 过滤器经由 executedContext.Exception 主路径采集（异常不会逸出到宿主）
                    services.Configure<MvcOptions>(mvcOptions =>
                    {
                        mvcOptions.Filters.Add(new TestUnhandledExceptionFilter());
                    });
                    services.AddSingleton<ISqlSugarClient>(_db);
                    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                    services.AddSingleton<ICache>(new FakeCache());
                    services.AddSingleton<SystemCacheService>();
                    services.AddScoped<SystemConfigService>();
                    services.AddSingleton<PlatformLoggingConfigProvider>();
                    services.AddSingleton<TenantDbConnectionProvider>();
                    services.AddSingleton<ISqlSugarTenantIdResolver>(new NullTenantIdResolver());
                    services.AddScoped(typeof(SqlSugarRepository<>));
                    services.AddSingleton<IEventPublisher, CaptureEventPublisher>();
                    services.AddElasticSearch();
                    services.AddLoggingSetup();
                });
                webBuilder.Configure(app =>
                {
                    // 测试身份：按请求头注入 Claims（无 JWT），覆盖 SuperAdmin / 租户 / 匿名三种上下文
                    app.Use(async (context, next) =>
                    {
                        context.User = BuildTestPrincipal(context.Request.Headers["X-Test-Identity"].ToString());
                        await next();
                    });
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                });
            })
            .Build();

        await host.StartAsync();
        return host;
    }

    private async Task<HttpStatusCode> PostAsync(string path, string identity, string json)
    {
        using (var client = _host.GetTestClient())
        using (var request = new HttpRequestMessage(HttpMethod.Post, path))
        {
            request.Headers.Add("X-Test-Identity", identity);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            var statusCode = response.StatusCode;
            await WaitDispatchDrainedAsync();
            return statusCode;
        }
    }

    /// <summary>
    /// 等待调度队列排空：真实落库/写文件完成后才做行断言（失败时超时让测试显式红）。
    /// </summary>
    private async Task WaitDispatchDrainedAsync()
    {
        using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
        {
            var dispatchQueue = _host.Services.GetRequiredService<ILoggingDispatchQueue>();
            await dispatchQueue.WaitDrainedAsync(cancellationTokenSource.Token);
        }
    }

    private static ClaimsPrincipal BuildTestPrincipal(string identity)
    {
        var claims = new List<Claim>();
        if (identity == SuperAdminIdentity)
        {
            claims.Add(new Claim(ClaimConst.Account, "superadmin"));
            claims.Add(new Claim(ClaimConst.RealName, "平台管理员"));
            claims.Add(new Claim(ClaimConst.UserId, "1"));
            claims.Add(new Claim(ClaimConst.AccountType, ((int)AccountTypeEnum.SuperAdmin).ToString()));
        }
        else if (identity == TenantIdentity)
        {
            claims.Add(new Claim(ClaimConst.Account, "tenant-admin"));
            claims.Add(new Claim(ClaimConst.RealName, "租户管理员"));
            claims.Add(new Claim(ClaimConst.UserId, "2001"));
            claims.Add(new Claim(ClaimConst.TenantId, TenantConfigId));
            claims.Add(new Claim(ClaimConst.AccountType, ((int)AccountTypeEnum.NormalUser).ToString()));
        }

        // 匿名：无 Claims 的未认证身份（authorizationClaims 输出 null）
        return new ClaimsPrincipal(new ClaimsIdentity(claims, TestSchemaName));
    }

    private DbConnectionConfig CreateDbConfig(string configId)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"gfx_monitor_{configId}_{Guid.NewGuid():N}.db");
        _dbPaths.Add(dbPath);
        var config = new DbConnectionConfig
        {
            ConfigId = configId,
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={dbPath}",
            IsAutoCloseConnection = true,
            DbSettings = new DbSettings { EnableInitDb = true, },
            TableSettings = new TableSettings { EnableInitTable = true, EnableIncreTable = false, },
            SeedSettings = new SeedSettings { EnableInitSeed = false, EnableIncreSeed = false, },
        };
        SqlSugarSetup.SetDbConfig(config);
        return config;
    }

    /// <summary>
    /// 无环境租户解析器：租户归属只来自日志 claims，与 Writer 独立作用域语义一致。
    /// </summary>
    private sealed class NullTenantIdResolver : ISqlSugarTenantIdResolver
    {
        public long? GetTenantId()
        {
            return null;
        }
    }

    /// <summary>
    /// 测试异常过滤器：模拟 AdminResultFilter 消化未处理异常（生产拓扑），
    /// 否则 TestServer 会把异常直接重抛给客户端而不是返回 500。
    /// </summary>
    private sealed class TestUnhandledExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            context.Result = new ObjectResult(new { Message = context.Exception.Message, })
            {
                StatusCode = StatusCodes.Status500InternalServerError,
            };
            context.ExceptionHandled = true;
        }
    }

    /// <summary>
    /// 事件发布 fake：记录异常邮件事件是否被触发（不真实发送）。
    /// </summary>
    private sealed class CaptureEventPublisher : IEventPublisher
    {
        public List<string> PublishedEventIds { get; } = new();

        public Task PublishAsync(string eventId, object? payload, CancellationToken cancellationToken = default)
        {
            lock (PublishedEventIds)
            {
                PublishedEventIds.Add(eventId);
            }

            return Task.CompletedTask;
        }
    }
}
