namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// Admin 平台配置错误码（段 305，见 <see cref="ErrorSegmentRegistry"/>；C265 自旧 D 码体系平移）。
/// </summary>
public enum PlatformConfigErrorCode
{
    /// <summary>
    /// 已存在同名或同编码参数配置
    /// </summary>
    [ErrorText("已存在同名或同编码参数配置", "A parameter with the same name or code already exists")]
    ParameterAlreadyExists = 30500001,

    /// <summary>
    /// 禁止删除系统参数
    /// </summary>
    [ErrorText("禁止删除系统参数", "Deleting system parameters is forbidden")]
    DeleteSystemParameterForbidden = 30500002,
}