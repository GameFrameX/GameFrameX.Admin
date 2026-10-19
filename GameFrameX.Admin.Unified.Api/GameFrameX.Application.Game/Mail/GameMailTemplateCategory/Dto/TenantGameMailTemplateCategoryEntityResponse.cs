namespace GameFrameX.Application.Game.Mail.GameMailTemplateCategory.Dto;

/// <summary>
/// 邮件模板分类输出参数
/// </summary>
public class TenantGameMailTemplateCategoryEntityResponse : IBasePageResponse<Entity.Tenant.Game.TenantGameMailTemplateCategoryEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// 应用Id。应用内容作用域（TenantId + AppId），必须 &gt; 0。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; } = null!;

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// 行版本号（乐观锁）
    /// </summary>
    public long? RowVersion { get; set; }
}