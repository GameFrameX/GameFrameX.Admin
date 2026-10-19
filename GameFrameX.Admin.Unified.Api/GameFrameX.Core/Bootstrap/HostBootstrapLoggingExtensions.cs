using Microsoft.AspNetCore.Builder;
using GameFrameX.Foundation.Logger;

namespace GameFrameX.Core.Bootstrap;

/// <summary>
/// 三 host（Client / Hub / Game）共享的启动日志初始化扩展（C261，GFX-668）。
/// <para>统一 <see cref="LauncherOptions"/> 驱动口径：Loki 标签（ServerType/TagName）、LogType、
/// IsGrafanaLoki / IsConsole / GrafanaLokiUrl 全部取自启动参数，替代各 Program.cs 内联副本
/// （Hub 收敛前硬编码关闭 Loki / 控制台，属口径漂移，未配置时默认值同为 false，行为不变）。</para>
/// <para>必须在 host 创建日志写入器之前、构建 <see cref="LauncherOptions"/> 之后调用（紧跟 OptionsBuilder 之后）。</para>
/// </summary>
public static class HostBootstrapLoggingExtensions
{
    /// <summary>
    /// 初始化共享日志管道：写入 Loki 标签、推导 LogType，并按启动参数创建日志写入器。
    /// <para>TagName 优先取同名环境变量，缺失（null 或空白）时回退进程友好名，保证多实例部署下日志可按进程区分。</para>
    /// </summary>
    /// <param name="builder">host 构建器（保留参数以贴合调用点语义，当前不读取额外状态）。</param>
    /// <param name="launcherOptions">进程启动选项，提供日志管道开关与 Loki endpoint。</param>
    public static void UseHostBootstrapLogging(this WebApplicationBuilder builder, LauncherOptions launcherOptions)
    {
        var tagName = Environment.GetEnvironmentVariable("TagName");
        if (string.IsNullOrWhiteSpace(tagName))
        {
            tagName = AppDomain.CurrentDomain.FriendlyName;
        }

        LogOptions.Default.GrafanaLokiLabels.Add("ServerType", AppDomain.CurrentDomain.FriendlyName);
        LogOptions.Default.GrafanaLokiLabels.Add("TagName", tagName);
        LogOptions.Default.LogType = tagName;

        if (!string.IsNullOrEmpty(launcherOptions.GrafanaLokiUrl))
        {
            LogOptions.Default.GrafanaLokiUrl = launcherOptions.GrafanaLokiUrl;
        }

        LogOptions.Default.IsGrafanaLoki = launcherOptions.IsGrafanaLoki;
        LogOptions.Default.IsConsole = launcherOptions.IsConsole;
        LogHandler.Create(LogOptions.Default);
    }
}