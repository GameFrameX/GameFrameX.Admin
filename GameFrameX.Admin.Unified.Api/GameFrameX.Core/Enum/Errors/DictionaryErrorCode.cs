namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// Admin 字典错误码（段 302，见 <see cref="ErrorSegmentRegistry"/>；C265 自旧 D 码体系平移）。
/// </summary>
public enum DictionaryErrorCode
{
    /// <summary>
    /// 字典类型不存在
    /// </summary>
    [ErrorText("字典类型不存在", "Dictionary type does not exist")]
    DictionaryTypeNotFound = 30200001,

    /// <summary>
    /// 字典类型已存在,名称或编码重复
    /// </summary>
    [ErrorText("字典类型已存在,名称或编码重复", "Dictionary type already exists (duplicate name or code)")]
    DictionaryTypeAlreadyExists = 30200002,

    /// <summary>
    /// 字典值已存在,名称或编码重复
    /// </summary>
    [ErrorText("字典值已存在,名称或编码重复", "Dictionary value already exists (duplicate name or code)")]
    DictionaryDataAlreadyExists = 30200003,

    /// <summary>
    /// 字典值不存在
    /// </summary>
    [ErrorText("字典值不存在", "Dictionary value does not exist")]
    DictionaryDataNotFound = 30200004,
}