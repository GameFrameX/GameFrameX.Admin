namespace GameFrameX.Application.NoticeCenter.GameMailTemplateCategory.Dto;

/// <summary>
/// 邮件模板分类分页查询输入参数
/// </summary>
public class PageTenantGameMailTemplateCategoryEntityRequest : BasePageRequest
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }
}