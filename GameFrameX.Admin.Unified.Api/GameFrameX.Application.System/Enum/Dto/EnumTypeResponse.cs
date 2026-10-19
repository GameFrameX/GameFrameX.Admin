namespace GameFrameX.Application.System.Enum.Dto;

/// <summary>
/// 枚举类型输出参数
/// </summary>
public class EnumTypeResponse
{
    /// <summary>
    /// 枚举类型描述
    /// </summary>
    public string TypeDescribe { get; set; } = string.Empty;

    /// <summary>
    /// 枚举类型名称
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// 枚举类型备注
    /// </summary>
    public string TypeRemark { get; set; } = string.Empty;
}