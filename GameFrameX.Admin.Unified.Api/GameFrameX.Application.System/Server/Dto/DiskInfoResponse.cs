namespace GameFrameX.Application.System.Server.Dto;

/// <summary>
/// 服务器磁盘信息
/// </summary>
public class DiskInfoResponse
{
    /// <summary>
    /// 磁盘名（Unix 为设备名及挂载点，Windows 为盘符）
    /// </summary>
    public string DiskName { get; set; } = null!;

    /// <summary>
    /// 文件系统类型名
    /// </summary>
    public string TypeName { get; set; } = null!;

    /// <summary>
    /// 总剩余空间（GB）
    /// </summary>
    public long TotalFree { get; set; }

    /// <summary>
    /// 总容量（GB）
    /// </summary>
    public long TotalSize { get; set; }

    /// <summary>
    /// 已使用空间（GB）
    /// </summary>
    public long Used { get; set; }

    /// <summary>
    /// 可用剩余空间（GB）
    /// </summary>
    public long AvailableFreeSpace { get; set; }

    /// <summary>
    /// 使用百分比（%）
    /// </summary>
    public decimal AvailablePercent { get; set; }
}