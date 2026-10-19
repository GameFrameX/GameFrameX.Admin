namespace GameFrameX.Application.System.GlobalLanguage.Dto;

/// <summary>
/// 本地化更新输入参数
/// </summary>
public class UpdateGlobalLanguageRequest : BaseUpdateRequest
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
}