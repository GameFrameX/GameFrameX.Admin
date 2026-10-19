namespace GameFrameX.Core.Service.File.Dto;

public class FileRequest : BaseIdRequest
{
    /// <summary>
    /// 文件名称
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// 文件Url
    /// </summary>
    public string? Url { get; set; }
}