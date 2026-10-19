namespace GameFrameX.Application.System.GlobalLanguage.Dto;

/// <summary>
/// 本地化增加输入参数
/// </summary>
public class AddGlobalLanguageRequest : BaseAddRequest
{
    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 详细描述
    /// </summary>
    [Required(ErrorMessage = "详细描述不能为空")]
    public string? Description { get; set; }

    /// <summary>
    /// 语言代码，ISO 3166 双字母国家/地区代码
    /// </summary>
    [Required(ErrorMessage = "语言代码，ISO 3166 双字母国家/地区代码不能为空")]
    public string? Code { get; set; }
}