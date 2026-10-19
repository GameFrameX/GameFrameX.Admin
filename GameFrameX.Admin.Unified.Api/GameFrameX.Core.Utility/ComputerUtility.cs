using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
// [Upgrade] 历史框架.RemoteRequest 已移除，改用 HttpClient
// using 历史框架.RemoteRequest.Extensions;
using GameFrameX.Core.Extension;

namespace GameFrameX.Core.Utility;

public static class ComputerUtility
{
    /// <summary>
    /// 内存信息
    /// </summary>
    /// <returns></returns>
    public static MemoryMetrics GetComputerInfo()
    {
        var client = new MemoryMetricsClient();
        var memoryMetrics = IsUnix() ? client.GetUnixMetrics() : client.GetWindowsMetrics();

        memoryMetrics.FreeRam = FormatGb(memoryMetrics.Free);
        memoryMetrics.UsedRam = FormatGb(memoryMetrics.Used);
        memoryMetrics.TotalRam = FormatGb(memoryMetrics.Total);
        memoryMetrics.RamRate = memoryMetrics.Total <= 0 ? "0%" : Math.Ceiling(100 * memoryMetrics.Used / memoryMetrics.Total).ToString(CultureInfo.InvariantCulture) + "%";
        memoryMetrics.CpuRate = Math.Ceiling(GetCPURate().ParseToDouble()).ToString(CultureInfo.InvariantCulture) + "%";
        return memoryMetrics;
    }

    /// <summary>
    /// 磁盘信息
    /// </summary>
    /// <returns></returns>
    public static List<DiskInfo> GetDiskInfos()
    {
        List<DiskInfo> diskInfos = new();

        if (IsUnix())
        {
            diskInfos.AddRange(GetUnixDiskInfos());
        }
        else
        {
            diskInfos.AddRange(GetDriveInfoDiskInfos());
        }

        return diskInfos;
    }

    /// <summary>
    /// 外网 IP 缓存秒数
    /// </summary>
    private const int RemoteIpCacheSeconds = 300;

    private static string? _cachedRemoteIp;
    private static DateTime _cachedRemoteIpAtUtc;
    private static readonly SemaphoreSlim _remoteIpLock = new(1, 1);

    /// <summary>
    /// 异步获取外网 IP 地址（带 5 分钟内存缓存；超时由调用方注入的 HttpClient 控制）。
    /// </summary>
    /// <param name="httpClient">由 <see cref="IHttpClientFactory"/> 创建、已配置超时的 HttpClient。</param>
    /// <returns>外网 IP 文本；不可用时返回“未知”。</returns>
    public static async Task<string> GetIpFromOnlineAsync(HttpClient httpClient)
    {
        if (httpClient == null)
        {
            throw new ArgumentNullException(nameof(httpClient));
        }

        if (IsRemoteIpCacheFresh(out var cachedIp))
        {
            return cachedIp;
        }

        await _remoteIpLock.WaitAsync();
        try
        {
            if (IsRemoteIpCacheFresh(out cachedIp))
            {
                return cachedIp;
            }

            try
            {
                var html = await httpClient.GetStringAsync("http://myip.ipip.net");
                var ip = !html.Contains("当前 IP：") ? "未知" : html.Replace("当前 IP：", "").Replace("来自于：", "");
                _cachedRemoteIp = ip;
                _cachedRemoteIpAtUtc = DateTime.UtcNow;
                return ip;
            }
            catch
            {
                // 网络抖动时回退到最近一次缓存（若有），避免 IP 立即恢复为“未知”
                return cachedIp ?? "未知";
            }
        }
        finally
        {
            _remoteIpLock.Release();
        }
    }

    private static bool IsRemoteIpCacheFresh([NotNullWhen(true)] out string? ip)
    {
        ip = _cachedRemoteIp;
        return ip != null && (DateTime.UtcNow - _cachedRemoteIpAtUtc).TotalSeconds < RemoteIpCacheSeconds;
    }

    /// <summary>
    /// 仅供单元测试重置外网 IP 缓存，避免用例间静态状态污染。
    /// </summary>
    internal static void ResetRemoteIpCacheForTests()
    {
        _cachedRemoteIp = null;
        _cachedRemoteIpAtUtc = DateTime.MinValue;
    }

    public static bool IsUnix()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }

    public static string GetCPURate()
    {
        string cpuRate;
        if (IsUnix())
        {
            var output = ShellUtil.Bash("top -b -n1 | grep \"Cpu(s)\" | awk '{print $2 + $4}'").Trim();
            cpuRate = double.TryParse(output, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ? parsed.ToString(CultureInfo.InvariantCulture) : "0";
        }
        else
        {
            var output = ShellUtil.Cmd("wmic", "cpu get LoadPercentage");
            cpuRate = output.Replace("LoadPercentage", string.Empty).Trim();
        }

        return cpuRate;
    }

    /// <summary>
    /// 获取系统运行时间
    /// </summary>
    /// <returns></returns>
    public static string GetRunTime()
    {
        var runTime = string.Empty;
        if (IsUnix())
        {
            var output = ShellUtil.Bash("uptime -s").Trim();
            runTime = DateTimeUtility.FormatTime((DateTime.Now - output.ParseToDateTime()).TotalMilliseconds.ToString().Split('.')[0].ParseToLong());
        }
        else
        {
            var output = ShellUtil.Cmd("wmic", "OS get LastBootUpTime/Value");
            var outputArr = output.Split('=', (char)StringSplitOptions.RemoveEmptyEntries);
            if (outputArr.Length == 2)
            {
                runTime = DateTimeUtility.FormatTime((DateTime.Now - outputArr[1].Split('.')[0].ParseToDateTime()).TotalMilliseconds.ToString().Split('.')[0].ParseToLong());
            }
        }

        return runTime;
    }

    private static string FormatGb(double megabytes)
    {
        if (megabytes <= 0)
        {
            return "0GB";
        }

        return Math.Round(megabytes / 1024, 2).ToString(CultureInfo.InvariantCulture) + "GB";
    }

    private static IEnumerable<DiskInfo> GetUnixDiskInfos()
    {
        var diskInfos = new List<DiskInfo>();
        var output = ShellUtil.Bash("df -mT -P 2>/dev/null | tail -n +2");
        var disks = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var seenMounts = new HashSet<string>(StringComparer.Ordinal);
        var ignoredTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "tmpfs",
            "devtmpfs",
            "proc",
            "sysfs",
            "devpts",
            "cgroup",
            "cgroup2",
            "overlayfs",
            "squashfs",
        };

        foreach (var item in disks)
        {
            var disk = item.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (disk.Length < 7 || ignoredTypes.Contains(disk[1]))
            {
                continue;
            }

            var mountPoint = disk[6];
            if (!seenMounts.Add(mountPoint))
            {
                continue;
            }

            if (!long.TryParse(disk[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var totalMb) ||
                !long.TryParse(disk[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var usedMb) ||
                !long.TryParse(disk[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out var freeMb))
            {
                continue;
            }

            diskInfos.Add(new DiskInfo
            {
                DiskName = mountPoint == "/" ? disk[0] : $"{disk[0]} ({mountPoint})",
                TypeName = disk[1],
                TotalSize = ToGb(totalMb),
                Used = ToGb(usedMb),
                AvailableFreeSpace = ToGb(freeMb),
                AvailablePercent = ParsePercent(disk[5], totalMb, usedMb),
            });
        }

        return diskInfos.Count > 0 ? diskInfos : GetDriveInfoDiskInfos();
    }

    private static IEnumerable<DiskInfo> GetDriveInfoDiskInfos()
    {
        foreach (var item in DriveInfo.GetDrives().Where(u => u.IsReady))
        {
            if (item.DriveType == DriveType.CDRom || item.TotalSize <= 0)
            {
                continue;
            }

            var totalSize = item.TotalSize / 1024 / 1024 / 1024;
            var availableFreeSpace = item.AvailableFreeSpace / 1024 / 1024 / 1024;
            var used = totalSize - availableFreeSpace;
            yield return new DiskInfo
            {
                DiskName = item.Name,
                TypeName = item.DriveType.ToString(),
                TotalSize = totalSize,
                AvailableFreeSpace = availableFreeSpace,
                Used = used,
                AvailablePercent = totalSize <= 0 ? 0 : decimal.Ceiling(used / (decimal)totalSize * 100),
            };
        }
    }

    private static long ToGb(long megabytes)
    {
        return megabytes <= 0 ? 0 : Math.Max(1, megabytes / 1024);
    }

    private static decimal ParsePercent(string percent, long totalMb, long usedMb)
    {
        if (decimal.TryParse(percent.Replace("%", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return totalMb <= 0 ? 0 : decimal.Ceiling(usedMb / (decimal)totalMb * 100);
    }
}