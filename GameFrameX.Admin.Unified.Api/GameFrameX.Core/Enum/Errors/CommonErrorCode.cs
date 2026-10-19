namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// 协议域通用错误码（段 200，见 <see cref="ErrorSegmentRegistry"/>）。
/// </summary>
public enum CommonErrorCode
{
    /// <summary>
    /// 没有找到
    /// </summary>
    [ErrorText("没有找到", "Not found")] NotFound = 20000001,

    /// <summary>
    /// 参数校验失败
    /// </summary>
    [ErrorText("参数校验失败", "Parameter validation failed")]
    ParameterValidationFailed = 20000002,

    /// <summary>
    /// 记录不存在
    /// </summary>
    [ErrorText("记录不存在", "Record does not exist")]
    RecordNotFound = 20000003,

    /// <summary>
    /// 数据已存在
    /// </summary>
    [ErrorText("数据已存在", "Data already exists")]
    RecordAlreadyExists = 20000004,

    /// <summary>
    /// 数据已被其他用户修改，请刷新后重试
    /// </summary>
    [ErrorText("数据已被其他用户修改，请刷新后重试", "The data was modified by another user; refresh and retry")]
    ConcurrencyConflict = 20000005,
}