using GameFrameX.Core.Bootstrap;
using GameFrameX.Core.SqlSugar;
using Serilog;

namespace GameFrameX.Web.Api
{
    public static class WebComponent
    {
        public static void Load(WebApplicationBuilder builder, LauncherOptions launcherOptions)
        {
            AddProjectJsonConfiguration(builder);

            // 清掉 CreateBuilder 默认 providers（Console/Debug/EventSource）：运行日志统一由 Foundation Serilog 管道输出，
            // 消除 MEL Console 与 Serilog Console 双写；OTLP provider 由下方 UseHostOpenTelemetry 重新注册，不受影响。
            builder.Logging.ClearProviders();

            // Grafana Cloud OpenTelemetry（OTLP 三信号：traces/metrics/logs，logs 自动入 Loki）；凭证齐全才启用
            builder.UseHostOpenTelemetry("GameFrameX.Web.Api");

            // Foundation 日志管道（C271，与 Client/Hub 同口径）：LogHandler.Create 建立 Serilog sinks（文件/控制台/Loki），
            // AddSerilog 把 MEL ILogger<T> 全量路由进 Serilog，运行日志统一受 Foundation LogOptions 管控。
            // Web.Api 是容器主力 host，docker logs 依赖 stdout，控制台无条件开启。
            launcherOptions.IsConsole = true;
            builder.UseHostBootstrapLogging(launcherOptions);
            builder.Services.AddSerilog();

            // DataProtection 密钥环持久化到 data/DataProtection-Keys（容器内挂载卷 /app/data，与 data/logs 同口径）：
            // 默认落 /home/app/.aspnet 容器重建即丢，且启动报 "may not be persisted" 告警
            builder.UseHostDataProtection();

            // 设置日志过滤（对 OTLP/Serilog provider 均生效；Serilog 侧 LogHandler 的 MinimumLevel.Override 已有同语义默认值）
            builder.Logging.AddFilter((provider, category, logLevel) => { return !new[] { "Microsoft.Hosting", "Microsoft.AspNetCore", }.Any(u => category?.StartsWith(u) ?? false) && logLevel >= LogLevel.Information; });

            // 设置接口超时时间和上传大小
            var hostBuilder = builder.Configuration.Get<WebHostBuilder>();
            if (hostBuilder != null)
            {
                hostBuilder.ConfigureKestrel(u =>
                {
                    u.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(30);
                    u.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(30);
                    u.Limits.MaxRequestBodySize = null;
                });
            }

            // 打印监听端口
            var urls = builder.WebHost.GetSetting("urls") ?? "http://*:20802";
            Console.WriteLine($"\n═══════════════════════════════════════════");
            Console.WriteLine($"  API 已启动，监听端口: {urls}");
            Console.WriteLine($"  Swagger: {urls}/swagger");
            Console.WriteLine($"═══════════════════════════════════════════\n");
        }

        private static void AddProjectJsonConfiguration(WebApplicationBuilder builder)
        {
            var directories = builder.Configuration.GetSection("ConfigurationScanDirectories").Get<string[]>() ?? Array.Empty<string>();
            var loadedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var roots = new[]
            {
                builder.Environment.ContentRootPath,
                AppContext.BaseDirectory,
            };

            foreach (var directory in directories.Where(directory => !string.IsNullOrWhiteSpace(directory)))
            {
                foreach (var root in roots.Where(Directory.Exists).Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    var path = Path.Combine(root, directory);
                    if (!Directory.Exists(path))
                    {
                        continue;
                    }

                    foreach (var file in Directory.EnumerateFiles(path, "*.json", SearchOption.TopDirectoryOnly).OrderBy(u => u, StringComparer.OrdinalIgnoreCase))
                    {
                        if (loadedFiles.Add(Path.GetFullPath(file)))
                        {
                            builder.Configuration.AddJsonFile(file, false, true);
                        }
                    }
                }
            }
        }
    }
}
