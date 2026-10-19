namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// Admin 区域错误码（段 308，见 <see cref="ErrorSegmentRegistry"/>；C265 自旧 D 码体系平移）。
/// </summary>
public enum RegionErrorCode
{
    /// <summary>
    /// 当前节点Id不能与父节点Id相同
    /// </summary>
    [ErrorText("当前节点Id不能与父节点Id相同", "Node ID cannot equal its parent ID")]
    NodeIdEqualsParentForbidden = 30800001,

    /// <summary>
    /// 已有相同编码或名称
    /// </summary>
    [ErrorText("已有相同编码或名称", "A region with the same code or name already exists")]
    RegionAlreadyExists = 30800002,
}