namespace GameFrameX.Core.Service.File.Dto;

public class FileResponse
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 提供者
    /// </summary>
    public string Provider { get; set; } = null!;

    /// <summary>
    /// 名称
    /// </summary>
    public string Name
    {
        get { return Id + Suffix; }
    }

    /// <summary>
    /// URL
    /// </summary>
    public string Url { get; set; } = null!;

    /// <summary>
    /// 大小
    /// </summary>
    public string SizeKb { get; set; } = null!;

    /// <summary>
    /// 后缀
    /// </summary>
    public string Suffix { get; set; } = null!;

    /// <summary>
    /// 路径
    /// </summary>
    public string FilePath { get; set; } = null!;

    /// <summary>
    /// 文件名称
    /// </summary>
    public string FileName { get; set; } = null!;
}