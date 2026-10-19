using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Mail.GameMailTemplate.Dto;

/// <summary>
/// 邮件模板新增/更新共有输入参数。
/// </summary>
public class TenantGameMailTemplateEntityBaseRequest : BaseAddRequest
{
    /// <summary>
    /// 应用Id。邮件模板属于应用内容，必须 &gt; 0。
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
    /// 分类Id
    /// </summary>
    [Required(ErrorMessage = "分类不能为空")]
    public long CategoryId { get; set; }

    /// <summary>
    /// 标题模板
    /// </summary>
    [Required(ErrorMessage = "标题模板不能为空")]
    public string TitleTemplate { get; set; } = null!;

    /// <summary>
    /// 副标题模板
    /// </summary>
    public string SubTitleTemplate { get; set; } = null!;

    /// <summary>
    /// 内容模板
    /// </summary>
    [Required(ErrorMessage = "内容模板不能为空")]
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
    /// 是否启用,true:启用，false:禁用，null:未设置(启用)
    /// </summary>
    public bool? IsEnabled { get; set; }
}