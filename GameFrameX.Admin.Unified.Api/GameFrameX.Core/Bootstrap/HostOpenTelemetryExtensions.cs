using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace GameFrameX.Core.Bootstrap;

/// <summary>
/// 三 host（Web / Client / Hub）共享的 Grafana Cloud OpenTelemetry 接入扩展。
/// <para>OTLP 统一三信号出口：Traces / Metrics / Logs 全部经 Grafana Cloud OTLP endpoint 导出，
/// 其中 Logs 信号由 Grafana Cloud 侧自动路由进 Loki，无需单独配置 Loki push；日志自动携带
/// trace_id / span_id，可在 Grafana 中与链路直接关联跳转。</para>
/// <para>启用条件：配置节 <c>OpenTelemetry</c> 的 Endpoint / UserName / ApiKey 三者齐全（建议经环境变量
/// <c>OpenTelemetry__Endpoint</c> 等注入，凭证不进 git）；任一缺失时整段跳过，行为与未接入前一致。</para>
/// </summary>
public static class HostOpenTelemetryExtensions
{
    /// <summary>
    /// 注册 OpenTelemetry（Traces + Metrics + Logs）并导出到 Grafana Cloud OTLP endpoint。
    /// </summary>
    /// <param name="builder">host 构建器。</param>
    /// <param name="serviceName">服务名（OTel resource <c>service.name</c>，Loki/Grafana 中即 <c>service_name</c> 标签），如 GameFrameX.Web.Api。</param>
    public static WebApplicationBuilder UseHostOpenTelemetry(this WebApplicationBuilder builder, string serviceName)
    {
        var section = builder.Configuration.GetSection("OpenTelemetry");
        var endpoint = section["Endpoint"];
        var userName = section["UserName"];
        var apiKey = section["ApiKey"];

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("[OpenTelemetry] 未启用：OpenTelemetry:Endpoint/UserName/ApiKey 任一缺失（环境变量 OpenTelemetry__* 注入齐全后随 Host 启动生效）");
            return builder;
        }

        // Grafana Cloud OTLP 认证：Basic auth（用户名 = Stack Instance ID，密码 = Grafana Cloud API Key / glc_ token）
        var basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes(userName + ":" + apiKey));

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName)
                .AddAttributes(new[]
                {
                    new KeyValuePair<string, object>("deployment.environment", builder.Environment.EnvironmentName),
                }))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(options => Configure(options, "/v1/traces")))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(options => Configure(options, "/v1/metrics")));

        // ILogger → OTLP logs（Grafana Cloud 侧自动入 Loki）
        builder.Logging.AddOpenTelemetry(options => { options.AddOtlpExporter(otlpOptions => Configure(otlpOptions, "/v1/logs")); });
        // OTel 日志导出 Information 起步，避免框架 Trace/Debug 日志刷量
        builder.Logging.AddFilter("OpenTelemetry", LogLevel.Information);
        // OTLP exporter 自身 HttpClient 每个导出周期打 4 行 info（Start/Sending/Received/End），压到 Warning 消控制台与 Loki 刷屏
        builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);

        Console.WriteLine($"[OpenTelemetry] 已启用：{serviceName} -> {endpoint}（Grafana Cloud OTLP，logs 自动入 Loki）");
        return builder;

        // 代码显式赋值的 Endpoint 按"最终 URL"处理（不追加信号路径，POST 到 base 会 404），故各信号显式拼 /v1/{signal}
        void Configure(OtlpExporterOptions options, string signalPath)
        {
            options.Protocol = OtlpExportProtocol.HttpProtobuf; // HTTPS/protobuf 兼容性最好（gRPC 需 h2，Docker / 受限网络易踩坑）
            options.Endpoint = new Uri(endpoint.TrimEnd('/') + signalPath);
            options.Headers = "Authorization=Basic " + basicAuth;
        }
    }
}
