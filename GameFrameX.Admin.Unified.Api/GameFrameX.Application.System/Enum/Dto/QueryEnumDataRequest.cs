namespace GameFrameX.Application.System.Enum.Dto;

public class QueryEnumDataRequest
{
    /// <summary>
    /// 实体名称
    /// </summary>
    /// <example>SystemUserEntity</example>
    [Required(ErrorMessage = "实体名称不能为空")]
    public string EntityName { get; set; } = string.Empty;

    /// <summary>
    /// 字段名称
    /// </summary>
    /// <example>AccountType</example>
    [Required(ErrorMessage = "字段名称不能为空")]
    public string FieldName { get; set; } = string.Empty;
}