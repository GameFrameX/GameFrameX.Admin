using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Mail.GameMailTemplate.Dto;

/// <summary>
/// 邮件模板输出参数
/// </summary>
public class TenantGameMailTemplateEntityResponse : IBasePageResponse<Entity.Tenant.Game.TenantGameMailTemplateEntity>
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
    /// 分类Id
    /// </summary>
    public long CategoryId { get; set; }

    /// <summary>
    /// 标题模板
    /// </summary>
    public string TitleTemplate { get; set; } = null!;

    /// <summary>
    /// 副标题模板
    /// </summary>
    public string SubTitleTemplate { get; set; } = null!;

    /// <summary>
    /// 内容模板（支持{{变量名}}占位符）
    /// </summary>
    public string BodyTemplate { get; set; } = null!;

    /// <summary>
    /// 默认附件JSON
    /// </summary>
    public string DefaultAttachmentsJson { get; set; } = null!;

    /// <summary>
    /// 默认邮件类型
    /// </summary>
    public GameMailType? DefaultMailType { get; set; }

    /// <summary>
    /// 默认过期天数
    /// </summary>
    public int? DefaultExpirationDays { get; set; }

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

    /// <summary>
    /// 分类名称
    /// </summary>
    public string CategoryIdName { get; set; } = null!;
}