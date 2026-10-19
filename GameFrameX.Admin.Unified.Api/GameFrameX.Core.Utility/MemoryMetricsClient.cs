// [Upgrade] 历史框架.RemoteRequest 已移除，改用 HttpClient
// using 历史框架.RemoteRequest.Extensions;

namespace GameFrameX.Core.Utility;

public class MemoryMetricsClient
{
    /// <summary>
    /// windows系统获取内存信息
    /// </summary>
    /// <returns></returns>
    public MemoryMetrics GetWindowsMetrics()
    {
        var output = ShellUtil.Cmd("wmic", "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value");
        var metrics = new MemoryMetrics();
        var lines = output.Trim().Split('\n', (char)StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 0)
        {
            return metrics;
        }

        var freeMemoryParts = lines[0].Split('=', (char)StringSplitOptions.RemoveEmptyEntries);
        var totalMemoryParts = lines[1].Split('=', (char)StringSplitOptions.RemoveEmptyEntries);

        metrics.Total = Math.Round(double.Parse(totalMemoryParts[1]) / 1024, 0);
        metrics.Free = Math.Round(double.Parse(freeMemoryParts[1]) / 1024, 0); //m
        metrics.Used = metrics.Total - metrics.Free;

        return metrics;
    }

    /// <summary>
    /// Unix系统获取
    /// </summary>
    /// <returns></returns>
    public MemoryMetrics GetUnixMetrics()
    {
        var output = ShellUtil.Bash("free -m | awk '{print $2,$3,$4,$5,$6}'");
        var metrics = new MemoryMetrics();
        var lines = output.Split('\n', (char)StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 0)
        {
            return metrics;
        }

        // 修复：检查 lines 长度是否足够访问索引 1
        if (lines != null && lines.Length > 1)
        {
            var memory = lines[1].Split(' ', (char)StringSplitOptions.RemoveEmptyEntries);
            if (memory.Length >= 3)
            {
                metrics.Total = double.Parse(memory[0]);
                metrics.Used = double.Parse(memory[1]);
                metrics.Free = double.Parse(memory[2]); //m
            }
        }

        return metrics;
    }
}