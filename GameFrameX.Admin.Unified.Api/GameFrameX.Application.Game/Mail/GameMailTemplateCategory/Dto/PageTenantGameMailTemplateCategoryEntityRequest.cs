namespace GameFrameX.Application.Game.Mail.GameMailTemplateCategory.Dto;

/// <summary>
/// 邮件模板分类分页查询输入参数
/// </summary>
public class PageTenantGameMailTemplateCategoryEntityRequest : BasePageRequest
{
    /// <summary>
    /// 应用Id
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }
}