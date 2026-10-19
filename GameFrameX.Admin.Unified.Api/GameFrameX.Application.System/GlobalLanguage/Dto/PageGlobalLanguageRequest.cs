namespace GameFrameX.Application.System.GlobalLanguage.Dto;

/// <summary>
/// 语言管理分页查询输入参数
/// </summary>
public class PageGlobalLanguageRequest : BasePageRequest
{
    /// <summary>
    /// 详细描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 语言代码，ISO 3166 双字母国家/地区代码
    /// </summary>
    public string? Code { get; set; }
}