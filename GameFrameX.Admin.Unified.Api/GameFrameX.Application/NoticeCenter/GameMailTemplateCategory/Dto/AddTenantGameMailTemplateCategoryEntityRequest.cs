namespace GameFrameX.Application.NoticeCenter.GameMailTemplateCategory.Dto;

/// <summary>
/// 邮件模板分类增加输入参数
/// </summary>
public class AddTenantGameMailTemplateCategoryEntityRequest : BaseAddRequest
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; } = null!;

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否启用,true:启用，false:禁用，null:未设置(启用)
    /// </summary>
    public bool? IsEnabled { get; set; }
}