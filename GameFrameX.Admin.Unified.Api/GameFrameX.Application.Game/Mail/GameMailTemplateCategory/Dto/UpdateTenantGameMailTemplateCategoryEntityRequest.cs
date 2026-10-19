namespace GameFrameX.Application.Game.Mail.GameMailTemplateCategory.Dto;

/// <summary>
/// 邮件模板分类更新输入参数
/// </summary>
public class UpdateTenantGameMailTemplateCategoryEntityRequest : BaseUpdateRequest
{
    /// <summary>
    /// 应用Id。不可变，更新时由服务端从既有实体回填。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }

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