namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// Admin 文件错误码（段 310，见 <see cref="ErrorSegmentRegistry"/>；C265 自旧 D 码体系平移）。
/// </summary>
public enum FileErrorCode
{
    /// <summary>
    /// 文件不存在
    /// </summary>
    [ErrorText("文件不存在", "File does not exist")]
    FileNotFound = 31000001,

    /// <summary>
    /// 不允许的文件类型
    /// </summary>
    [ErrorText("不允许的文件类型", "File type is not allowed")]
    FileTypeNotAllowed = 31000002,

    /// <summary>
    /// 文件超过允许大小
    /// </summary>
    [ErrorText("文件超过允许大小", "File exceeds the allowed size")]
    FileSizeExceeded = 31000003,

    /// <summary>
    /// 文件后缀错误
    /// </summary>
    [ErrorText("文件后缀错误", "File extension is invalid")]
    FileExtensionInvalid = 31000004,
}