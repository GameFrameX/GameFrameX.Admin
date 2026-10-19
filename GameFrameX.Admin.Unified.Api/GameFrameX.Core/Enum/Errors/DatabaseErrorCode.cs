namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// Admin 代码生成数据库表错误码（段 312，见 <see cref="ErrorSegmentRegistry"/>；C265 自旧 D 码体系平移）。
/// </summary>
public enum DatabaseErrorCode
{
    /// <summary>
    /// 请添加数据列
    /// </summary>
    [ErrorText("请添加数据列", "Please add data columns")]
    ColumnsRequired = 31200001,

    /// <summary>
    /// 不允许添加相同字段名
    /// </summary>
    [ErrorText("不允许添加相同字段名", "Duplicate field names are not allowed")]
    DuplicateFieldNameForbidden = 31200002,
}