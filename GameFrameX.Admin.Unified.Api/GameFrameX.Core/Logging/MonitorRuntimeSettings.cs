using Microsoft.Extensions.Logging;

namespace GameFrameX.Core.Logging;

/// <summary>
/// Monitor 采集结构性运行参数（C171：随 Host 重启生效，不做运行中热更新）。
/// </summary>
internal sealed record MonitorRuntimeSettings
{
    public MonitorRuntimeSettings(bool globalEnabled, bool withReturnValue, int returnValueThreshold, LogLevel bahLogLevel)
    {
        GlobalEnabled = globalEnabled;
        WithReturnValue = withReturnValue;
        ReturnValueThreshold = returnValueThreshold;
        BahLogLevel = bahLogLevel;
    }

    /// <summary>全局拦截开关（false 时整个 Monitor 采集停摆，等价旧 Logging.json Monitor.GlobalEnabled）。</summary>
    public bool GlobalEnabled { get; }

    /// <summary>是否记录返回值。</summary>
    public bool WithReturnValue { get; }

    /// <summary>返回值字符串阈值（0=全量，超长截断）。</summary>
    public int ReturnValueThreshold { get; }

    /// <summary>数据校验类（validation）日志输出级别。</summary>
    public LogLevel BahLogLevel { get; }
}
