namespace GameFrameX.Core.Service.File.Dto;

public class PageFileRequest : BasePageRequest
{
    /// <summary>
    /// 文件名称
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// 开始时间
    /// </summary>
    public long? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public long? EndTime { get; set; }
}