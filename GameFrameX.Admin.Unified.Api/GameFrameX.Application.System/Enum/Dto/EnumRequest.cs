namespace GameFrameX.Application.System.Enum.Dto;

/// <summary>
/// 枚举输入参数
/// </summary>
public class EnumRequest
{
    /// <summary>
    /// 枚举类型名称
    /// </summary>
    /// <example>AccountTypeEnum</example>
    [Required(ErrorMessage = "枚举类型不能为空")]
    public string EnumName { get; set; } = string.Empty;
}