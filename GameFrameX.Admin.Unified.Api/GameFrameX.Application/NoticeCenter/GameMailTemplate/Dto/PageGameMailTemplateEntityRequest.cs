namespace GameFrameX.Application.NoticeCenter.GameMailTemplate.Dto;

/// <summary>
/// 邮件模板分页查询输入参数
/// </summary>
public class PageGameMailTemplateEntityRequest : BasePageRequest
{
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