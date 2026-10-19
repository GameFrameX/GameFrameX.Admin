namespace GameFrameX.Core.Service.File.Dto;

public class UploadFileFromBase64Request
{
    /// <summary>
    /// 文件内容
    /// </summary>
    public string FileDataBase64 { get; set; } = null!;

    /// <summary>
    /// 文件类型( "image/jpeg",)
    /// </summary>
    public string ContentType { get; set; } = null!;

    /// <summary>
    /// 文件名称
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// 保存路径
    /// </summary>
    public string Path { get; set; } = null!;
}