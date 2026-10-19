using System.Text.Json.Serialization;
// [Upgrade] 历史框架.RemoteRequest 已移除，改用 HttpClient
// using 历史框架.RemoteRequest.Extensions;

namespace GameFrameX.Core.Utility;

/// <summary>
/// 内存信息
/// </summary>
public class MemoryMetrics
{
    [JsonIgnore] public double Total { get; set; }

    [JsonIgnore] public double Used { get; set; }

    [JsonIgnore] public double Free { get; set; }

    /// <summary>
    /// 已用内存
    /// </summary>
    public string UsedRam { get; set; } = string.Empty;

    /// <summary>
    /// CPU使用率%
    /// </summary>
    public string CpuRate { get; set; } = string.Empty;

    /// <summary>
    /// 总内存 GB
    /// </summary>
    public string TotalRam { get; set; } = string.Empty;

    /// <summary>
    /// 内存使用率 %
    /// </summary>
    public string RamRate { get; set; } = string.Empty;

    /// <summary>
    /// 空闲内存
    /// </summary>
    public string FreeRam { get; set; } = string.Empty;
}