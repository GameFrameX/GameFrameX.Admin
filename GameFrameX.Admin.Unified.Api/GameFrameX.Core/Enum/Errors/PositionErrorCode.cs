namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// Admin 职位错误码（段 309，见 <see cref="ErrorSegmentRegistry"/>；C265 自旧 D 码体系平移）。
/// </summary>
public enum PositionErrorCode
{
    /// <summary>
    /// 已存在同名或同编码职位
    /// </summary>
    [ErrorText("已存在同名或同编码职位", "A position with the same name or code already exists")]
    PositionAlreadyExists = 30900001,

    /// <summary>
    /// 该职位下有用户禁止删除
    /// </summary>
    [ErrorText("该职位下有用户禁止删除", "The position has users and cannot be deleted")]
    PositionHasUsersDeleteForbidden = 30900002,

    /// <summary>
    /// 无权修改本职位
    /// </summary>
    [ErrorText("无权修改本职位", "No permission to modify this position")]
    NoPermissionForPosition = 30900003,

    /// <summary>
    /// 职位不存在
    /// </summary>
    [ErrorText("职位不存在", "Position does not exist")]
    PositionNotFound = 30900004,
}