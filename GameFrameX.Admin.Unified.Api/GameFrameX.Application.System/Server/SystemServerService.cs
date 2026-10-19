using System.Diagnostics;
using System.Runtime.InteropServices;
using AngleSharp.Html.Parser;
using GameFrameX.Application.System.Server.Dto;
using Lazy.Captcha.Core;
using MiniExcelLibs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Elastic.Clients.Elasticsearch;
using SKIT.FlurlHttpClient.Wechat.Api;
using SKIT.FlurlHttpClient.Wechat.TenpayV3;
using UAParser;
using GameFrameX.Foundation.Http.Normalization;
using GameFrameX.Foundation.Utility.DistributedSystem.Snowflake;
using GameFrameX.Email.Smtp;

namespace GameFrameX.Application.System.Server;

/// <summary>
/// 系统服务器监控服务
/// </summary>
[ApiDescriptionSettings(Order = 290)]
public class SystemServerService : IAdminApiController
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IHttpClientFactory _httpClientFactory;

    public SystemServerService(IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment webHostEnvironment,
        IHostEnvironment hostEnvironment,
        IHttpClientFactory httpClientFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _webHostEnvironment = webHostEnvironment;
        _hostEnvironment = hostEnvironment;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// 获取服务器配置信息
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取服务器配置信息")]
    [ApiDescriptionSettings(Name = "GetServerBase")]
    public async Task<HttpJsonResultData<ServerBaseResponse>> GetServerBaseAsync()
    {
        var remoteIp = await ComputerUtility.GetIpFromOnlineAsync(_httpClientFactory.CreateClient("remoteIp"));
        return HttpJsonResultData<ServerBaseResponse>.Success(new ServerBaseResponse
        {
            HostName = Environment.MachineName, // 主机名称
            SystemOs = RuntimeInformation.OSDescription, // 操作系统
            OsArchitecture = Environment.OSVersion.Platform.ToString() + " " + RuntimeInformation.OSArchitecture.ToString(), // 系统架构
            ProcessorCount = Environment.ProcessorCount + " 核", // CPU核心数
            SystemRunTime = ComputerUtility.GetRunTime(), // 系统运行时间
            RemoteIp = remoteIp, // 外网地址
            LocalIp = _httpContextAccessor.HttpContext?.Connection?.LocalIpAddress?.ToString(), // 本地地址
            FrameworkDescription = RuntimeInformation.FrameworkDescription, // NET框架
            Environment = _hostEnvironment.IsDevelopment() ? "Development" : "Production",
            Wwwroot = _webHostEnvironment.WebRootPath, // 网站根目录
            Stage = _hostEnvironment.IsStaging() ? "Stage环境" : "非Stage环境", // 是否Stage环境
        });
    }

    /// <summary>
    /// 获取服务器使用信息
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取服务器使用信息")]
    [ApiDescriptionSettings(Name = "GetServerUsed")]
    public Task<HttpJsonResultData<ServerUsedResponse>> GetServerUsedAsync()
    {
        var programStartTime = Process.GetCurrentProcess().StartTime;
        var totalMilliseconds = (DateTime.Now - programStartTime).TotalMilliseconds.ToString();
        var ts = totalMilliseconds.Contains('.') ? totalMilliseconds.Split('.')[0] : totalMilliseconds;
        var programRunTime = DateTimeUtility.FormatTime(ts.ParseToLong());

        var memoryMetrics = ComputerUtility.GetComputerInfo();
        return Task.FromResult(HttpJsonResultData<ServerUsedResponse>.Success(new ServerUsedResponse
        {
            FreeRam = memoryMetrics.FreeRam, // 空闲内存
            UsedRam = memoryMetrics.UsedRam, // 已用内存
            TotalRam = memoryMetrics.TotalRam, // 总内存
            RamRate = memoryMetrics.RamRate, // 内存使用率
            CpuRate = memoryMetrics.CpuRate, // Cpu使用率
            StartTime = programStartTime.ToString("yyyy-MM-dd HH:mm:ss"), // 服务启动时间
            RunTime = programRunTime, // 服务运行时间
        }));
    }

    /// <summary>
    /// 获取服务器磁盘信息
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取服务器磁盘信息")]
    [ApiDescriptionSettings(Name = "GetServerDisk")]
    public Task<HttpJsonResultData<List<DiskInfoResponse>>> GetServerDiskAsync()
    {
        return Task.FromResult(HttpJsonResultData<List<DiskInfoResponse>>.Success(ComputerUtility.GetDiskInfos()
                                                                                                 .Select(disk => new DiskInfoResponse
                                                                                                 {
                                                                                                     DiskName = disk.DiskName,
                                                                                                     TypeName = disk.TypeName,
                                                                                                     TotalFree = disk.TotalFree,
                                                                                                     TotalSize = disk.TotalSize,
                                                                                                     Used = disk.Used,
                                                                                                     AvailableFreeSpace = disk.AvailableFreeSpace,
                                                                                                     AvailablePercent = disk.AvailablePercent,
                                                                                                 })
                                                                                                 .ToList()));
    }

    /// <summary>
    /// 获取框架主要程序集
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取框架主要程序集")]
    [ApiDescriptionSettings(Name = "GetAssemblyList")]
    public Task<HttpJsonResultData<List<AssemblyInfoResponse>>> GetAssemblyListAsync()
    {
        var projectCoreAssembly = typeof(ProjectTypeCatalog).Assembly.GetName();
        var sqlSugarAssembly = typeof(ISqlSugarClient).Assembly.GetName();
        var snowFlakeAssembly = typeof(SnowFlakeIdHelper).Assembly.GetName();
        var cacheAssembly = typeof(Microsoft.Extensions.Caching.Memory.MemoryCache).Assembly.GetName();
        var jsonAssembly = typeof(JsonSerializer).Assembly.GetName();
        var excelAssembly = typeof(MiniExcel).Assembly.GetName();
        var captchaAssembly = typeof(ICaptcha).Assembly.GetName();
        var wechatApiAssembly = typeof(WechatApiClient).Assembly.GetName();
        var wechatTenpayAssembly = typeof(WechatTenpayClient).Assembly.GetName();
        // [Upgrade] OSS 临时禁用
        // var ossAssembly = typeof(IOSSServiceFactory).Assembly.GetName();
        var parserAssembly = typeof(Parser).Assembly.GetName();
        // [Upgrade] NEST 已由 Elastic.Clients.Elasticsearch 替代
        var esAssembly = typeof(ElasticsearchClient).Assembly.GetName();
        var rateLimitAssembly = typeof(global::System.Threading.RateLimiting.FixedWindowRateLimiter).Assembly.GetName();
        var htmlParserAssembly = typeof(HtmlParser).Assembly.GetName();
        var fluentEmailAssembly = typeof(SmtpEmailSender).Assembly.GetName();

        return Task.FromResult(HttpJsonResultData<List<AssemblyInfoResponse>>.Success(new List<AssemblyInfoResponse>
        {
            new() { Name = projectCoreAssembly.Name, Version = projectCoreAssembly.Version, },
            new() { Name = sqlSugarAssembly.Name, Version = sqlSugarAssembly.Version, },
            new() { Name = snowFlakeAssembly.Name, Version = snowFlakeAssembly.Version, },
            new() { Name = cacheAssembly.Name, Version = cacheAssembly.Version, },
            new() { Name = jsonAssembly.Name, Version = jsonAssembly.Version, },
            new() { Name = excelAssembly.Name, Version = excelAssembly.Version, },
            new() { Name = captchaAssembly.Name, Version = captchaAssembly.Version, },
            new() { Name = wechatApiAssembly.Name, Version = wechatApiAssembly.Version, },
            new() { Name = wechatTenpayAssembly.Name, Version = wechatTenpayAssembly.Version, },
            // [Upgrade] ossAssembly 临时禁用
            // new AssemblyInfoResponse { Name = ossAssembly.Name, Version = ossAssembly.Version, },
            new() { Name = parserAssembly.Name, Version = parserAssembly.Version, },
            new() { Name = esAssembly.Name, Version = esAssembly.Version, },
            new() { Name = rateLimitAssembly.Name, Version = rateLimitAssembly.Version, },
            new() { Name = htmlParserAssembly.Name, Version = htmlParserAssembly.Version, },
            new() { Name = fluentEmailAssembly.Name, Version = fluentEmailAssembly.Version, },
        }));
    }
}