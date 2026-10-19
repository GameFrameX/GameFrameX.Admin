namespace GameFrameX.Application.Game.Mail.GameMailTemplate.Dto;

/// <summary>
/// 邮件模板分页查询输入参数
/// </summary>
public class PageTenantGameMailTemplateEntityRequest : BasePageRequest
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
    /// 分类Id
    /// </summary>
    public long? CategoryId { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }
}