using System.Collections.Concurrent;
using GameFrameX.Core.Service.PlatformLogging;
using GameFrameX.Entity.Enum;
using Microsoft.Extensions.Logging;

namespace GameFrameX.Core.Logging;

/// <summary>
/// Monitor 采集运行时状态：首次使用时从 <see cref="PlatformLoggingConfigProvider"/> 快照读取一次
/// <c>platform_logging_config</c> 的 Monitor 分组并缓存整个 Host 生命周期——结构性参数（GlobalEnabled /
/// WithReturnValue / 返回值阈值 / BahLogLevel）随重启生效，这是 C171 既定的语义边界：
/// 只有 database.enabled / elasticSearch.enabled 属于「保存后同 Host 即时生效」的写路径开关。
/// </summary>
internal sealed class MonitorLoggingRuntimeState
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<bool, MonitorRuntimeSettings> _cache = new();

    public MonitorLoggingRuntimeState(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 获取本 Host 生命周期内唯一的 Monitor 运行时参数（首次调用时读取快照后固定）。
    /// </summary>
    public MonitorRuntimeSettings GetSettings()
    {
        return _cache.GetOrAdd(true, _ => ReadSettings());
    }

    private MonitorRuntimeSettings ReadSettings()
    {
        try
        {
            var monitor = _serviceProvider.GetRequiredService<PlatformLoggingConfigProvider>().GetSnapshot().Monitor;
            return new MonitorRuntimeSettings(
                monitor.GlobalEnabled,
                monitor.WithReturnValue,
                monitor.ReturnValueThreshold,
                ParseLogLevel(monitor.BahLogLevel));
        }
        catch (Exception exception) when (exception is InvalidOperationException or NotSupportedException)
        {
            // 快照缺失 Monitor 分组（历史配置行）或 Provider 未注册：退回安全默认值，监控不因此拒止
            return new MonitorRuntimeSettings(true, true, 0, LogLevel.Information);
        }
    }

    private static LogLevel ParseLogLevel(string levelName)
    {
        return System.Enum.TryParse<LogLevel>(levelName, true, out var level) ? level : LogLevel.Information;
    }
}
