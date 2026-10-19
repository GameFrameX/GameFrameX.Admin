namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// Admin 打印模板错误码（段 315，见 <see cref="ErrorSegmentRegistry"/>；C265 自旧 D 码体系平移）。
/// </summary>
public enum PrintErrorCode
{
    /// <summary>
    /// 已存在同名打印模板
    /// </summary>
    [ErrorText("已存在同名打印模板", "A print template with the same name already exists")]
    PrintTemplateAlreadyExists = 31500001,
}