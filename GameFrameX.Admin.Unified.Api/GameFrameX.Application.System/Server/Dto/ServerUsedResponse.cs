namespace GameFrameX.Application.System.Server.Dto;

/// <summary>
/// 服务器使用信息
/// </summary>
public class ServerUsedResponse
{
    /// <summary>
    /// 空闲内存
    /// </summary>
    public string FreeRam { get; set; } = null!;

    /// <summary>
    /// 已用内存
    /// </summary>
    public string UsedRam { get; set; } = null!;

    /// <summary>
    /// 总内存
    /// </summary>
    public string TotalRam { get; set; } = null!;

    /// <summary>
    /// 内存使用率
    /// </summary>
    public string RamRate { get; set; } = null!;

    /// <summary>
    /// CPU 使用率
    /// </summary>
    public string CpuRate { get; set; } = null!;

    /// <summary>
    /// 服务启动时间（yyyy-MM-dd HH:mm:ss）
    /// </summary>
    public string StartTime { get; set; } = null!;

    /// <summary>
    /// 服务运行时间
    /// </summary>
    public string RunTime { get; set; } = null!;
}